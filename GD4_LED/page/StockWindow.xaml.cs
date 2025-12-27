using GD4_LED.cls;
using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GD4_LED.page
{
    public partial class StockWindow : Page, INotifyPropertyChanged
    {
        public ObservableCollection<DrugStockGroupModel> DrugStocks { get; set; }
        private ObservableCollection<DrugStockGroupModel> AllDrugStocks { get; set; }
        private DataTable dt;              // DataTable เก็บข้อมูลทั้งหมด
        private DataTable dtFiltered;      // DataTable ผลลัพธ์หลังกรอง

        private string _currentFilter = "ALL"; // เก็บสถานะ filter ปัจจุบัน

        public int TotalCount => DrugStocks?.Count ?? 0;
        public int LowStockCount => DrugStocks?.Count(x => x.TotalQuantity < x.min) ?? 0;

        // นับยาใกล้หมดอายุภายใน 6 เดือน
        public int NearExpiryCount => DrugStocks?.Count(x =>
            x.LotDetails.Any(lot => IsNearExpiry(lot.exp))) ?? 0;

        public object SelectedDrug { get; set; }
        public int RefillQuantity { get; set; }
        public int Led_id { get; set; }
        public string RefillLot { get; set; }
        public DateTime? RefillExpiryDate { get; set; } = DateTime.Today.AddMonths(12);
        public string RefillNotes { get; set; }
        private bool _isLoading = true;
        private bool _isUpdating = false;
        private CancellationTokenSource _searchCancellationTokenSource;

        cls.clsQuery _query = new cls.clsQuery();

        public event PropertyChangedEventHandler PropertyChanged;

        public StockWindow()
        {
            // Initialize collections before InitializeComponent
            DrugStocks = new ObservableCollection<DrugStockGroupModel>();
            AllDrugStocks = new ObservableCollection<DrugStockGroupModel>();
            
            InitializeComponent();
            
            // Set DataContext after initialization
            this.DataContext = this;
            
            StartLoadingAnimation();
            _ = InitializePageAsync();
        }

        // ตรวจสอบว่ายาใกล้หมดอายุภายใน 6 เดือนหรือไม่
        private bool IsNearExpiry(string expDate)
        {
            if (string.IsNullOrEmpty(expDate)) return false;

            DateTime exp;
            if (DateTime.TryParse(expDate, out exp))
            {
                return (exp - DateTime.Today).TotalDays <= 180 && exp > DateTime.Today;
            }
            return false;
        }

        private async Task ConnectToDatabaseAsync()
        {
            try
            {
                StartLoadingAnimation();
                await Task.Run(() => { });
                StopLoadingAnimation();
            }
            catch (Exception ex)
            {
                StopLoadingAnimation();
                MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}");
            }
        }
        public void LoadStock()
        {
            bool v = false;
            v = _query.CheckConnection(clsvariable.connectionST);
            //MessageBox.Show(v.ToString());
            DataTable dt = new DataTable();
            dt = _query.GetAllLedStock(clsvariable.shelfzone);
            string connStr = GD4_LED.Properties.Settings.Default.connectstring;
            //MessageBox.Show(GD4_LED.Properties.Settings.Default.connectstring);
            if (dt.Rows.Count > 0)
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

                    string insertCols = string.Join(", ", columns);
                    string insertParams = string.Join(", ", columns.Select(c => "@" + c));

                    string updateCols = string.Join(", ", columns
                                                    .Where(c => c != "orderitemcode")
                                                    .Select(c => $"{c} = VALUES({c})"));

                    string sql = $@" INSERT INTO ms_stock ({insertCols}) VALUES ({insertParams}) ON DUPLICATE KEY UPDATE {updateCols}; ";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            cmd.Parameters.Clear();

                            foreach (DataColumn col in dt.Columns)
                            {
                                object value = row[col.ColumnName] ?? DBNull.Value;
                                cmd.Parameters.AddWithValue("@" + col.ColumnName, value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }

                    Console.WriteLine("✅ Insert/Update สำเร็จทุกแถว!");
                }

            }
            else
            {
                //MessageBox.Show("ไม่มีข้อมูลตำแหน่งยาในตู้ LED นี้", "ข้อมูลว่าง",
                //              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StartLoadingAnimation()
        {
            var loadingStoryboard = (Storyboard)this.Resources["LoadingAnimation"];
            loadingStoryboard.Begin();
            var fadeInStoryboard = (Storyboard)this.Resources["FadeInLoading"];
            fadeInStoryboard.Begin();
        }

        private void StopLoadingAnimation()
        {
            var fadeOutStoryboard = (Storyboard)this.Resources["FadeOutLoading"];
            fadeOutStoryboard.Begin();
        }

        private async Task InitializePageAsync()
        {
            try
            {
                var minimumLoadingTime = Task.Delay(500);
                var dataLoadingTask = LoadDataAsync();
                await Task.WhenAll(minimumLoadingTime, dataLoadingTask);
                await HideLoadingAndShowContent();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}",
                              "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                await HideLoadingAndShowContent();
            }
        }

        private async Task LoadDataAsync()
        {
            await Task.Run(() =>
            {
                DataTable dt = _query.GetLedStock(clsvariable.shelfzone);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetupSmoothScrolling();
                    
                    string jsonData = JsonConvert.SerializeObject(dt, Formatting.Indented);
                    var rawData = JsonConvert.DeserializeObject<List<DrugStockModel>>(jsonData);

                    var groupedData = GroupDrugsByCode(rawData);

                    // Clear and repopulate instead of creating new instances
                    AllDrugStocks.Clear();
                    DrugStocks.Clear();
                    
                    foreach (var item in groupedData)
                    {
                        AllDrugStocks.Add(item);
                        DrugStocks.Add(item);
                    }

                    // อัพเดทจำนวนสถิติหลังจากโหลดข้อมูลเสร็จ
                    UpdateCounts();
                }), System.Windows.Threading.DispatcherPriority.Background);
            });
        }

        // จัดกลุ่มยาที่มี code เดียวกันแต่ต่าง lot
        private List<DrugStockGroupModel> GroupDrugsByCode(List<DrugStockModel> rawData)
        {
            var grouped = rawData.GroupBy(x => x.drugCode)
                .Select(g => new DrugStockGroupModel
                {
                    drugCode = g.Key,
                    drugName = g.First().drugName,
                    drugPosition = g.First().drugPosition,
                    location = g.First().location,
                    min = (int)g.First().min,
                    max = (int)g.First().max,

                    // รวม Lot ทั้งหมด
                    LotDetails = g.Select(x => new LotDetail
                    {
                        lot = x.lot,
                        exp = x.exp,
                        Quantity = (int)x.Quantity
                    }).ToList(),

                    // คำนวณจำนวนรวม
                    TotalQuantity = (int)g.Sum(x => x.Quantity),

                    // คำนวณ Percent จากจำนวนรวมกับ max
                    
                    Percent = g.First().max > 0
    ? Math.Min((double)g.Sum(x => x.Quantity) / (int)g.First().max * 100, 100)
    : 0
                })
                .ToList();

            return grouped;
        }

        private async Task HideLoadingAndShowContent()
        {
            var loadingStoryboard = (Storyboard)this.Resources["LoadingAnimation"];
            loadingStoryboard.Stop();
            var fadeOutStoryboard = (Storyboard)this.Resources["FadeOutLoading"];
            fadeOutStoryboard.Begin();
            await Task.Delay(300);
            LoadingOverlay.Visibility = Visibility.Collapsed;
            var slideUpStoryboard = (Storyboard)this.Resources["SlideUpAnimation"];
            slideUpStoryboard.Begin();
            _isLoading = false;
        }

        private void SetupSmoothScrolling()
        {
            StockScrollViewer.ScrollChanged += StockScrollViewer_ScrollChanged;
            StockScrollViewer.PreviewMouseWheel += StockScrollViewer_PreviewMouseWheel;
        }

        private void StockScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                StockScrollViewer.InvalidateVisual();
            }
        }

        private void StockScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double scrollAmount = e.Delta > 0 ? -120 : 120;
                var animation = new DoubleAnimation()
                {
                    From = scrollViewer.VerticalOffset,
                    To = Math.Max(0, Math.Min(scrollViewer.ScrollableHeight,
                        scrollViewer.VerticalOffset + scrollAmount)),
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
                };
                scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
                e.Handled = true;
            }
        }

        // Event handler สำหรับ TextBox ค้นหา
        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Cancel previous search
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            var textBox = sender as TextBox;
            string keyword = textBox.Text.Trim().ToLower();

            // อัพเดท Placeholder
            SearchPlaceholder.Text = textBox.Text.Length > 0 ? "" : "Med code/name";

            // ถ้าไม่มีคำค้นหา ให้แสดงข้อมูลทั้งหมดตาม filter ปัจจุบัน
            if (string.IsNullOrWhiteSpace(keyword))
            {
                ApplyCurrentFilter();
                return;
            }

            // Debounce search
            try
            {
                await Task.Delay(300, token);
                
                if (token.IsCancellationRequested) return;

                await Task.Run(() =>
                {
                    var filtered = AllDrugStocks.Where(x =>
                        (x.drugCode != null && x.drugCode.ToLower().Contains(keyword)) ||
                        (x.drugName != null && x.drugName.ToLower().Contains(keyword))
                    ).ToList();

                    if (!token.IsCancellationRequested)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            DrugStocks.Clear();
                            foreach (var item in filtered)
                                DrugStocks.Add(item);

                            UpdateCounts();
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }
                }, token);
            }
            catch (TaskCanceledException)
            {
                // Ignore cancellation
            }
        }

        // Event handler สำหรับ ComboBox เลือกแถว
        private void RowSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RowSelector.SelectedItem == null) return;

            var selectedItem = RowSelector.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;

            string selectedContent = selectedItem.Content.ToString();

            // Clear search box
            SearchTextBox.Text = "";
            SearchPlaceholder.Text = "Med code/name";

            if (selectedContent == "ทั้งหมด")
            {
                _currentFilter = "ALL";
            }
            else
            {
                // Extract row number (e.g., "แถวที่ 1" -> "1")
                _currentFilter = selectedContent;
            }

            ApplyCurrentFilter();
        }

        // ใช้ filter ตามสถานะปัจจุบัน
        private void ApplyCurrentFilter()
        {
            if (_currentFilter == "ALL")
            {
                DrugStocks.Clear();
                foreach (var item in AllDrugStocks)
                    DrugStocks.Add(item);
            }
            else if (_currentFilter == "LowStock")
            {
                // Filter เฉพาะยาที่ต้องเติม
                var filtered = AllDrugStocks.Where(x => x.TotalQuantity < x.min).ToList();
                DrugStocks.Clear();
                foreach (var item in filtered)
                    DrugStocks.Add(item);
            }
            else if (_currentFilter == "NearExpiry")
            {
                // Filter เฉพาะยาที่ใกล้หมดอายุ
                var filtered = AllDrugStocks.Where(x =>
                    x.LotDetails.Any(lot => IsNearExpiry(lot.exp))
                ).ToList();
                DrugStocks.Clear();
                foreach (var item in filtered)
                    DrugStocks.Add(item);
            }
            else if (_currentFilter.StartsWith("แถวที่"))
            {
                // Extract row number
                string rowNumberStr = _currentFilter.Replace("แถวที่", "").Trim();
                if (int.TryParse(rowNumberStr, out int rowNumber))
                {
                    // Calculate shelf name range (1-8, 9-16, 17-24, etc.)
                    int startRange = (rowNumber - 1) * 8 + 1;
                    int endRange = rowNumber * 8;

                    var filtered = AllDrugStocks.Where(x =>
                    {
                        if (string.IsNullOrEmpty(x.drugPosition)) return false;

                        // Extract numeric part from shelf name (e.g., "D5" -> 5, "M29" -> 29)
                        string numericPart = new string(x.drugPosition.Where(char.IsDigit).ToArray());
                        if (int.TryParse(numericPart, out int shelfNumber))
                        {
                            return shelfNumber >= startRange && shelfNumber <= endRange;
                        }
                        return false;
                    }).ToList();

                    DrugStocks.Clear();
                    foreach (var item in filtered)
                        DrugStocks.Add(item);
                }
            }

            UpdateCounts();
        }

        // อัพเดทจำนวนต่างๆ
        private void UpdateCounts()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(NearExpiryCount));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Event handler สำหรับคลิก Card (แทนปุ่มเติมยา)
        private async void DrugCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                var border = sender as Border;
                var drugData = border?.DataContext;

                if (drugData != null)
                {
                    SelectedDrug = drugData;
                    DrugStockGroupModel drug = drugData as DrugStockGroupModel;

                    await Task.Run(async () =>
                    {
                        DataTable dt_stock = _query.GetLedStockByZoneCode(drug.drugCode, clsvariable.shelfzone);
                        
                        if (dt_stock.Rows.Count > 0 && 
                            dt_stock.Rows[0]["position_id"].ToString() != "" && 
                            dt_stock.Rows[0]["addr"].ToString() != "")
                        {
                            int position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                            int addr = Convert.ToInt32(dt_stock.Rows[0]["addr"].ToString());
                            string orderitemENname = dt_stock.Rows[0]["orderitemENname"].ToString();
                            string position = dt_stock.Rows[0]["shelfname"].ToString();
                            string HAD = string.IsNullOrEmpty(dt_stock.Rows[0]["HAD"].ToString()) ? "0" : dt_stock.Rows[0]["HAD"].ToString();
                            string qty = dt_stock.Rows[0]["In_Qty"].ToString();
                            string LotNo = dt_stock.Rows[0]["LotNo"].ToString();
                            string exp = dt_stock.Rows[0]["Exp"].ToString();

                            clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id, orderitemENname, " ", HAD, position);
                            await Task.Delay(2000);
                            clsvariable.Instance.SerialCan.Order(addr, position_id, " ", " ", LotNo, exp, qty, 0, 0, 0);
                            await Task.Delay(2000);
                        }
                    });
                }

                StartLoadingAnimation();
                await InitializePageAsync();
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // Event สำหรับปุ่มเติมยา (ซ่อนไว้แล้ว แต่เก็บไว้กรณีต้องการใช้)
        private async void RefillMedicine_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                var button = sender as Button;
                var drugData = button?.DataContext;

                if (drugData != null)
                {
                    SelectedDrug = drugData;
                    DrugStockGroupModel drug = drugData as DrugStockGroupModel;

                    await Task.Run(() =>
                    {
                        DataTable dt_stock = _query.GetLocation(drug.drugCode, clsvariable.shelfzone);
                        if (dt_stock.Rows.Count > 0 && 
                            dt_stock.Rows[0]["position_id"].ToString() != "" && 
                            dt_stock.Rows[0]["addr"].ToString() != "")
                        {
                            int position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                            int addr = Convert.ToInt32(dt_stock.Rows[0]["addr"].ToString());
                            clsvariable.Instance.SerialCan.SetLED(addr, position_id, 255, 0, 0);
                        }
                    });

                    await Task.Delay(2000);
                    ShowPopup(SelectedDrug);
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            try
            {
                var button = sender as Button;
                var drugData = button?.DataContext;

                if (drugData != null)
                {
                    SelectedDrug = drugData;
                    DrugStockGroupModel drug = drugData as DrugStockGroupModel;

                    await Task.Run(() =>
                    {
                        DataTable dt_stock = _query.GetLedStockByZoneCode(drug.drugCode, clsvariable.shelfzone);
                        
                        if (dt_stock.Rows.Count > 0 && 
                            dt_stock.Rows[0]["position_id"].ToString() != "" && 
                            dt_stock.Rows[0]["addr"].ToString() != "")
                        {
                            int position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                            int addr = Convert.ToInt32(dt_stock.Rows[0]["addr"].ToString());
                            string orderitemENname = dt_stock.Rows[0]["orderitemENname"].ToString();
                            string position = dt_stock.Rows[0]["shelfname"].ToString();
                            string HAD = string.IsNullOrEmpty(dt_stock.Rows[0]["HAD"].ToString()) ? "0" : dt_stock.Rows[0]["HAD"].ToString();
                            string qty = dt_stock.Rows[0]["In_Qty"].ToString();
                            string LotNo = dt_stock.Rows[0]["LotNo"].ToString();
                            string exp = dt_stock.Rows[0]["Exp"].ToString();

                            clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id, orderitemENname, " ", HAD, position);
                            Task.Delay(2000).Wait();
                            clsvariable.Instance.SerialCan.Order(addr, position_id, " ", " ", LotNo, exp, qty, 0, 0, 0);
                            Task.Delay(2000).Wait();
                        }
                    });
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // Event handlers สำหรับ Summary Cards
        private void LowStockCard_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFilter = "LowStock";
            SearchTextBox.Text = "";
            SearchPlaceholder.Text = "Med code/name";

            // Filter เฉพาะยาที่ต้องเติม (จำนวนน้อยกว่า min)
            var filtered = AllDrugStocks.Where(x => x.TotalQuantity < x.min).ToList();

            DrugStocks.Clear();
            foreach (var item in filtered)
                DrugStocks.Add(item);

            UpdateCounts();
        }

        private void NearExpiryCard_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFilter = "NearExpiry";
            SearchTextBox.Text = "";
            SearchPlaceholder.Text = "Med code/name";

            // Filter เฉพาะยาที่ใกล้หมดอายุภายใน 6 เดือน
            var filtered = AllDrugStocks.Where(x =>
                x.LotDetails.Any(lot => IsNearExpiry(lot.exp))
            ).ToList();

            DrugStocks.Clear();
            foreach (var item in filtered)
                DrugStocks.Add(item);

            UpdateCounts();
        }

        private void TotalCard_Click(object sender, MouseButtonEventArgs e)
        {
            _currentFilter = "ALL";
            SearchTextBox.Text = "";
            SearchPlaceholder.Text = "Med code/name";

            // แสดงทั้งหมด
            DrugStocks.Clear();
            foreach (var item in AllDrugStocks)
                DrugStocks.Add(item);

            UpdateCounts();
        }

        private void SaveRefillData()
        {
            var refillRecord = new
            {
                DrugCode = ((DrugStockGroupModel)SelectedDrug).drugCode,
                Quantity = RefillQuantity,
                LotNumber = RefillLot,
                Led_id = Led_id,
                ExpiryDate = RefillExpiryDate.Value,
                Notes = RefillNotes,
                RefillDate = DateTime.Now,
                UserId = "CurrentUser"
            };

            Console.WriteLine($"บันทึกการเติมยา: {refillRecord}");
        }

        private void ShowSuccessMessage()
        {
            string message = $"เติมยาสำเร็จ!\n\n" +
                            $"รหัสยา: {((DrugStockGroupModel)SelectedDrug).drugCode}\n" +
                            $"จำนวนที่เติม: {RefillQuantity:N0} หน่วย\n" +
                            $"Lot: {RefillLot}\n" +
                            $"วันหมดอายุ: {RefillExpiryDate:dd/MM/yyyy}";

            MessageBox.Show(message, "เติมยาสำเร็จ",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshDrugStocks()
        {
            Console.WriteLine("รีเฟรชข้อมูลสต็อกยา");
        }

        public void ShowPopup(object SelectedDrug)
        {
            //Window popupWindow = new Window
            //{
            //    Title = "Popup",
            //    Width = 800,
            //    Height = 1400,
            //    WindowStartupLocation = WindowStartupLocation.CenterScreen,
            //    WindowStyle = WindowStyle.None,
            //    ResizeMode = ResizeMode.NoResize,
            //    Topmost = true,
            //    AllowsTransparency = true,
            //    Background = Brushes.Transparent
            //};

            DrugStockGroupModel drug = SelectedDrug as DrugStockGroupModel;

            List<RefillRecord> refillList = new List<RefillRecord>
            {
                new RefillRecord
                {
                    DrugCode = drug.drugCode,
                    DrugName = drug.drugName,
                    Quantity = drug.TotalQuantity.ToString(),
                    Location = drug.location,
                    DrugPosition = drug.drugPosition,
                }
            };

            //PopupRefill popupPage = new PopupRefill(refillList);
                    var popup = new WinPopupRefill(refillList);
            //popup.Content = popupPage;
            popup.ShowDialog();

            //popupWindow.Content = popupPage;
            //popupWindow.ShowDialog();
        }

        public static class ScrollViewerBehavior
        {
            public static readonly DependencyProperty VerticalOffsetProperty =
                DependencyProperty.RegisterAttached("VerticalOffset", typeof(double),
                    typeof(ScrollViewerBehavior),
                    new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

            public static void SetVerticalOffset(FrameworkElement target, double value)
            {
                target.SetValue(VerticalOffsetProperty, value);
            }

            public static double GetVerticalOffset(FrameworkElement target)
            {
                return (double)target.GetValue(VerticalOffsetProperty);
            }

            private static void OnVerticalOffsetChanged(DependencyObject target,
                DependencyPropertyChangedEventArgs e)
            {
                ScrollViewer scrollViewer = target as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
                }
            }
        }
    }


}