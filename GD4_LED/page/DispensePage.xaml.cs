using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;
using GD4_LED.cls;
using GD4_LED.models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
using System.Windows.Threading;
using System.Xml.Linq;
using static GD4_LED.cls.ClsSubSerial;
using Border = System.Windows.Controls.Border;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using DrawingBitmap = System.Drawing.Bitmap;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;
namespace GD4_LED.page
{
    public partial class DispensePage : Page
    {
        //private readonly RxService _RX;
        private List<Prescription> allPrescriptions = new List<Prescription>();
        public List<Prescription> filteredPrescriptions = new List<Prescription>();
        //private RxService _RX;
        private bool _isLoading = true;
        clsQuery _query = new clsQuery();
        private DispatcherTimer timer;
        public static DispatcherTimer timerRefresh;
        private bool isVerified = false;
        clsvariable clsvariable = clsvariable.Instance;
        //private DispatcherTimer scanTimer;
        private string scannedBarcode = "";
        private DispatcherTimer searchDebounceTimer;
        private const int SEARCH_DEBOUNCE_MS = 300;
        private Dictionary<string, Prescription> prescriptionCache = new Dictionary<string, Prescription>();
        
        // PrescriptionPopup properties
        private HashSet<PackageItem> selectedItems = new HashSet<PackageItem>();
        private PrescriptionData prescriptionData;
        public DispatcherTimer timerBtn;
        private DispatcherTimer resh;
        bool Cradclick = false;
        DataTable db_print = new DataTable();
        string jsonString = "";
        private DispatcherTimer _focusTimer;
        private bool _allowFocusReturn = true;
        public bool PressBtn = false;
        private ClsSubSerial _serial;
        public DispensePage()
        {
            InitializeComponent();
            
            // Initialize db_print DataTable structure
            db_print.Columns.Add("prescriptionno", typeof(string));
            db_print.Columns.Add("hn", typeof(string));
            db_print.Columns.Add("an", typeof(string));
            db_print.Columns.Add("patientname", typeof(string));
            db_print.Columns.Add("wardname", typeof(string));
            db_print.Columns.Add("bedcode", typeof(string));
            db_print.Columns.Add("orderitemcode", typeof(string));
            db_print.Columns.Add("orderitemname", typeof(string));
            db_print.Columns.Add("orderqty", typeof(int));
            db_print.Columns.Add("shelfname", typeof(string));
            db_print.Columns.Add("addr", typeof(string));
            db_print.Columns.Add("position_id", typeof(string));

            // เริ่ม Loading Animation
            StartLoadingAnimation();

            // Initialize debounce timer for search
            searchDebounceTimer = new DispatcherTimer();
            searchDebounceTimer.Interval = TimeSpan.FromMilliseconds(SEARCH_DEBOUNCE_MS);
            searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            timerRefresh = new DispatcherTimer();
            timerRefresh.Interval = TimeSpan.FromSeconds(15);
            timerRefresh.Start();
            timerRefresh.Tick += Refresh_Tick;
            
            timerBtn = new DispatcherTimer();
            timerBtn.Interval = TimeSpan.FromSeconds(1);
            timerBtn.Tick += TimerBtn_Tick;
            
            // Timer สำหรับคืน Focus กลับมาเมื่อไม่มี interaction
            _focusTimer = new DispatcherTimer();
            _focusTimer.Interval = TimeSpan.FromSeconds(3); // คืน focus หลังจาก 3 วินาที
            _focusTimer.Tick += FocusTimer_Tick;
            
            this.Loaded += (s, e) =>
            {
                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
            };
            
            // เพิ่ม event เมื่อมีการคลิกที่ไหนก็ได้ในหน้า
            this.PreviewMouseDown += Page_PreviewMouseDown;
            
            this.KeyDown += Window_KeyDown;

            // โหลดข้อมูลแบบ Async
            _ = InitializePageAsync();

            //timerBtn.Start();

            _serial = new ClsSubSerial();
            _serial.OnButtonReceived += Serial_OnButtonReceived;
        }
        private void Serial_OnButtonReceived(object sender, ButtonEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {                
                PressBtn = true;
                clsvariable.CountItem++;

                //MessageBox.Show($"Serial_OnButtonReceived  ID:{e.Row}  Addr:{e.Addr}  QTY:{e.Qty}");
                InvenStock_Addr(e.Row, e.Addr, e.Qty.ToString());
                // ทำงานต่อใน Page ได้เต็มที่
                UpdateUI(e);
            });
        }
        private void UpdateUI(ButtonEventArgs e)
        {
            //MessageBox.Show($"UpdateUI  CountItem:{clsvariable.CountItem}  PackItem:{clsvariable.PackItem}");
            if (clsvariable.CountItem == clsvariable.PackItem)
            {
                ClosePrescriptionDetail();
                clsvariable.CountItem = 0;
                _ = InitializePageAsync();
            }
            else if (txtSelectedItems != null)
            {
                txtSelectedItems.Text = clsvariable.CountItem.ToString();
                //timerBtn.Start();
                PressBtn = false;
            }
        }
        private void Serial_OnDispense(object sender, ButtonEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                timerBtn.Stop();

                PressBtn = true;
                clsvariable.CountItem++;

                DataTable dt_stock = new clsQuery()
                    .GetLedStockByAddr(e.Row, e.Addr, clsvariable.shelfzone);
            });
        }
        private void TimerBtn_Tick(object sender, EventArgs e)
        {
            timerBtn.Stop();
            if (clsvariable.PackItem > 0)
            {
                //clsvariable.StrU_array[0] = "5";
                //clsvariable.StrU_array[1] = "4";
                //clsvariable.StrU_array[2] = "1";

                if (clsvariable.StrU_array[0] != "" && clsvariable.StrU_array[1] != "" && clsvariable.StrU_array[2] != "" && Convert.ToInt32(clsvariable.StrU_array[2]) > 0)
                {
                    //MessageBox.Show(clsvariable.press_btn.ToString());
                    //clsvariable.press_btn = true;
                    if (PressBtn)
                    {
                        //InvenStock_Addr(clsvariable.StrU_array[0], clsvariable.StrU_array[1], clsvariable.StrU_array[2]);
                        txtSelectedItems.Text = clsvariable.CountItem.ToString();

                        if (clsvariable.CountItem == clsvariable.PackItem)
                        {
                            ClosePrescriptionDetail();
                            clsvariable.CountItem = 0;
                            _ = InitializePageAsync();
                        }
                        else if (txtSelectedItems != null)
                        {
                            txtSelectedItems.Text = clsvariable.CountItem.ToString();
                            //timerBtn.Start();
                            PressBtn = false;
                        }

                        PressBtn = false;
                    }
                }
                else
                {
                    PressBtn = false;
                }

            }
            else
            {
                PressBtn = false;
            }

            clsvariable.StrU = "";
            clsvariable.StrU_array = new string[3];

            MessageBox.Show(PressBtn.ToString());
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(clsvariable.StrU) && clsvariable.StrU.Length >= 16 && clsvariable.StrU != "0")
            {
                DataTable dtPrescriptions = new DataTable();
                timer.Start();
                dtPrescriptions = _query.GetPrescription(clsvariable.shelfzone);
                if (dtPrescriptions.Rows.Count > 0)
                {
                    SearchTextBox.Text = dtPrescriptions.Rows[0]["prescriptionno"].ToString();
                }
                else
                {
                    SearchTextBox.Text = "";
                }


                if (SearchTextBox.Text != "")
                {
                    Prescription presc = DisplayPrescriptionsScanbarCode();
                    Print(presc);
                    timerRefresh.Stop();
                }

            }
            // โหลดข้อมูลแบบ Async
            //_ = InitializePageAsync();
            clsvariable.StrU = "0";
            timer.Start();
        }

        private void StartLoadingAnimation()
        {
            // เริ่ม Animation หมุนของไอคอน
            var loadingStoryboard = (Storyboard)this.Resources["LoadingAnimation"];
            loadingStoryboard.Begin();

            // เริ่ม Fade In Animation สำหรับ Loading
            var fadeInStoryboard = (Storyboard)this.Resources["FadeInLoading"];
            fadeInStoryboard.Begin();
        }

        public async Task InitializePageAsync()
        {
            try
            {
                // แสดง Loading อย่างน้อย 1.5 วินาที เพื่อให้เห็น Animation
                var minimumLoadingTime = Task.Delay(500);

                // โหลดข้อมูลจริง
                var dataLoadingTask = LoadDataAsync();

                // รอให้ทั้งสองงานเสร็จ
                await Task.WhenAll(minimumLoadingTime, dataLoadingTask);

                // ซ่อน Loading และแสดงเนื้อหาหลัก
                await HideLoadingAndShowContent();
            }
            catch (Exception ex)
            {
                // จัดการ Error
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}",
                              "ข้อผิดพลาด", MessageBoxButton.OK, MessageBoxImage.Error);
                await HideLoadingAndShowContent();
            }
        }

        private async Task LoadDataAsync()
        {
            // ใช้ Task.Run เพื่อไม่ให้ UI ค้าง
            await Task.Run(async () =>
            {
                // โหลดข้อมูลใน Background Thread
                await Task.Delay(100); // จำลองการโหลดข้อมูล

                // กลับมาที่ UI Thread เพื่ออัพเดท UI
                Dispatcher.Invoke(() =>
                {
                    SetupSmoothScrolling();
                    LoadPrescriptions();
                    UpdateStatistics();
                    //_RX = new RxService();

                    // Subscribe event
                    //_RX.OnTriggerReceived += Rx_OnTriggerReceived;
                });
            });
        }

        private async Task HideLoadingAndShowContent()
        {
            // หยุด Loading Animation
            var loadingStoryboard = (Storyboard)this.Resources["LoadingAnimation"];
            loadingStoryboard.Stop();

            // Fade Out Loading
            var fadeOutStoryboard = (Storyboard)this.Resources["FadeOutLoading"];
            fadeOutStoryboard.Begin();

            // รอให้ Fade Out เสร็จ
            await Task.Delay(300);

            // ซ่อน Loading Overlay
            LoadingOverlay.Visibility = Visibility.Collapsed;

            // เริ่ม Slide Up Animation สำหรับเนื้อหาหลัก
            var slideUpStoryboard = (Storyboard)this.Resources["SlideUpAnimation"];
            slideUpStoryboard.Begin();

            _isLoading = false;
            
            // Set focus to SearchTextBox after loading
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
                _focusTimer.Start(); // เริ่ม timer หลังจากโหลดเสร็จ
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }


        private void SetupSmoothScrolling()
        {
            // Enable smooth scrolling for touch devices
            MainScrollViewer.ScrollChanged += MainScrollViewer_ScrollChanged;

            // Add mouse wheel smooth scrolling
            MainScrollViewer.PreviewMouseWheel += MainScrollViewer_PreviewMouseWheel;
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // This helps with touch scrolling performance
            if (e.VerticalChange != 0)
            {
                MainScrollViewer.InvalidateVisual();
            }
        }

        private void MainScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Smooth mouse wheel scrolling
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double scrollAmount = e.Delta > 0 ? -120 : 120; // Adjust scroll speed

                // Create smooth scroll animation
                var animation = new DoubleAnimation()
                {
                    From = scrollViewer.VerticalOffset,
                    To = scrollViewer.VerticalOffset + scrollAmount,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
                };

                // Apply animation to ScrollViewer
                scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
                e.Handled = true;
            }
        }

        public void LoadPrescriptions()
        {
            DataTable dtPrescriptions = _query.GetPrescription(clsvariable.shelfzone);
            
            if (dtPrescriptions == null || dtPrescriptions.Rows.Count == 0)
            {
                allPrescriptions.Clear();
                filteredPrescriptions.Clear();
                DisplayPrescriptions();
                return;
            }

            // Optimize: Process data only once without redundant loop
            var prescriptions = dtPrescriptions.AsEnumerable()
                .GroupBy(r => r["prescriptionno"].ToString())
                .Select(g => new
                {
                    prescriptionno = g.Key,
                    hn = g.First()["hn"].ToString(),
                    an = g.First()["an"].ToString(),
                    patientname = g.First()["patientname"].ToString(),
                    ward = g.First()["ward"].ToString(),
                    bed = g.First()["bed"].ToString(),
                    status = "รอจัด",
                    package = g.Select(r => new
                    {
                        orderitemcode = r["orderitemcode"].ToString(),
                        orderitemname = r["orderitemname"].ToString(),
                        orderqty = Convert.ToInt32(r["orderqty"]),
                        addr = r["addr"].ToString(),
                        id = r["position_id"].ToString(),
                        location = r["shelfname"].ToString()
                    }).ToList()
                })
                .ToList();

            string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.None);
            allPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);
            
            // Update cache
            prescriptionCache.Clear();
            foreach (var p in allPrescriptions)
            {
                prescriptionCache[p.PrescriptionNo] = p;
            }
            
            filteredPrescriptions = allPrescriptions.ToList();
            DisplayPrescriptions();
        }

        private void LoadPrescriptionsByCode(string prescriptionno)
        {
            // Check cache first
            if (prescriptionCache.ContainsKey(prescriptionno))
            {
                allPrescriptions = new List<Prescription> { prescriptionCache[prescriptionno] };
                filteredPrescriptions = allPrescriptions.ToList();
                DisplayPrescriptions();
                return;
            }

            DataTable dtPrescriptions = _query.GetPrescriptionByCode(prescriptionno);
            clsvariable.dt_Prescr = dtPrescriptions;
            
            if (dtPrescriptions == null || dtPrescriptions.Rows.Count == 0)
            {
                return;
            }

            // Optimize: Remove redundant loop
            var prescriptions = dtPrescriptions.AsEnumerable()
                .GroupBy(r => r["prescriptionno"].ToString())
                .Select(g => new
                {
                    prescriptionno = g.Key,
                    hn = g.First()["hn"].ToString(),
                    an = g.First()["an"].ToString(),
                    patientname = g.First()["patientname"].ToString(),
                    ward = g.First()["wardname"].ToString(),
                    bed = g.First()["bedcode"].ToString(),
                    status = "รอจัด",
                    package = g.Select(r => new
                    {
                        orderitemcode = r["orderitemcode"].ToString(),
                        orderitemname = r["orderitemname"].ToString(),
                        orderqty = Convert.ToInt32(r["orderqty"]),
                        addr = r["addr"].ToString(),
                        id = r["position_id"].ToString(),
                        location = r["shelfname"].ToString()
                    }).ToList()
                })
                .ToList();

            string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.None);
            allPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);
            filteredPrescriptions = allPrescriptions.ToList();
            DisplayPrescriptions();
        }

        private void UpdateStatistics()
        {
            int pendingCount = filteredPrescriptions.Count(p => p.Status == "รอจัด");
            //int completedCount = filteredPrescriptions.Count(p => p.Status == "เสร็จแล้ว");
            int totalCount = filteredPrescriptions.Count;

            PendingCountText.Text = pendingCount.ToString();
            //CompletedCountText.Text = completedCount.ToString();
            //TotalCountText.Text = totalCount.ToString();
        }

        private void DisplayPrescriptions()
        {
            // Optimize: Use BeginInit/EndInit to batch UI updates
            PrescriptionPanel.Children.Clear();

            string Searchtxt = SearchTextBox.Text?.Trim() ?? "";

            if (filteredPrescriptions.Count == 0 && !string.IsNullOrEmpty(Searchtxt))
            {
                LoadPrescriptionsByCode(Searchtxt);
                return;
            }

            // Optimize: Suspend layout during bulk updates
            foreach (var prescription in filteredPrescriptions)
            {
                CreatePrescriptionCard(prescription);
            }
        }
        public Prescription DisplayPrescriptionsScanbarCode()
        {
            PrescriptionPanel.Children.Clear();
            Prescription prescrip = new Prescription();
            string Searchtxt = SearchTextBox.Text?.Trim() ?? "";

            if (filteredPrescriptions.Count == 0)
            {
                LoadPrescriptionsByCode(Searchtxt);
            }
            else
            {

            }

            foreach (var prescription in filteredPrescriptions)
            {
                prescrip = prescription;
                CreatePrescriptionCard(prescription);
            }

            return prescrip;
        }

        private void CreatePrescriptionCard(Prescription prescription)
        {
            System.Windows.Controls.Border cardBorder = new System.Windows.Controls.Border();
            cardBorder.Style = (Style)FindResource("CardStyle");
            cardBorder.Cursor = System.Windows.Input.Cursors.Hand;
            cardBorder.Margin = new Thickness(0, 0, 0, 12);

            Grid mainGrid = new Grid();

            // Column definitions
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Row definitions
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Patient info
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Pixel) }); // Spacing
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Prescription info
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Pixel) }); // Spacing
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Package count & buttons
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Medicine details (collapsible)

            // Patient Info
            StackPanel patientPanel = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetRow(patientPanel, 0);
            Grid.SetColumn(patientPanel, 0);

            // Status badge
            System.Windows.Controls.Border statusBorder = new System.Windows.Controls.Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4)
            };

            // Set color based on status
            //if (prescription.Status == "รอจัด")
            //{
            //    statusBorder.Background = (Brush)FindResource("Warning");
            //}
            //else if (prescription.Status == "เสร็จแล้ว")
            //{
            //    statusBorder.Background = (Brush)FindResource("Success");
            //}

            TextBlock statusText = new TextBlock
            {
                Text = prescription.Status,
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Height = 17
            };
            statusBorder.Child = statusText;

            TextBlock patientName = new TextBlock
            {
                Text = prescription.PatientName,
                Style = (Style)FindResource("SubHeaderTextStyle"),
                Margin = new Thickness(12, 0, 0, 0)
            };

            patientPanel.Children.Add(statusBorder);
            patientPanel.Children.Add(patientName);

            // Prescription Info
            Grid prescriptionGrid = new Grid();
            Grid.SetRow(prescriptionGrid, 2);
            Grid.SetColumn(prescriptionGrid, 0);

            prescriptionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            prescriptionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel leftInfo = new StackPanel();
            Grid.SetColumn(leftInfo, 0);

            leftInfo.Children.Add(new TextBlock
            {
                Text = $"เลขใบสั่งยา: {prescription.PrescriptionNo}",
                Style = (Style)FindResource("BodyTextStyle")
            });
            leftInfo.Children.Add(new TextBlock
            {
                Text = $"HN: {prescription.HN}",
                Style = (Style)FindResource("SecondaryTextStyle")
            });
            leftInfo.Children.Add(new TextBlock
            {
                Text = $"AN: {prescription.AN}",
                Style = (Style)FindResource("SecondaryTextStyle")
            });

            StackPanel rightInfo = new StackPanel();
            Grid.SetColumn(rightInfo, 1);

            rightInfo.Children.Add(new TextBlock
            {
                Text = $"หอผู้ป่วย: {prescription.Ward}",
                Style = (Style)FindResource("BodyTextStyle")
            });
            rightInfo.Children.Add(new TextBlock
            {
                Text = $"เตียง: {prescription.Bed}",
                Style = (Style)FindResource("SecondaryTextStyle")
            });

            prescriptionGrid.Children.Add(leftInfo);
            prescriptionGrid.Children.Add(rightInfo);

            // Package Count Summary and Action Buttons
            Grid packageAndButtonGrid = new Grid();
            Grid.SetRow(packageAndButtonGrid, 4);
            Grid.SetColumn(packageAndButtonGrid, 0);
            Grid.SetColumnSpan(packageAndButtonGrid, 2);

            packageAndButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            packageAndButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel packageSummary = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetColumn(packageSummary, 0);

            TextBlock packageCountText = new TextBlock
            {
                Text = $"รายการยา: {prescription.Package.Count} รายการ",
                Style = (Style)FindResource("BodyTextStyle"),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock expandIcon = new TextBlock
            {
                Text = "▼",
                FontSize = 30,
                Margin = new Thickness(8, 0, 0, 0),
                Foreground = (Brush)FindResource("TextSecondary"),
                Name = "ExpandIcon",
                VerticalAlignment = VerticalAlignment.Center
            };

            packageSummary.Children.Add(packageCountText);
            packageSummary.Children.Add(expandIcon);

            // Action Buttons Panel
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(buttonPanel, 1);

            // Print Button
            Button printButton = new Button
            {
                Content = "จัดยา",
                Width = 80,
                Height = 36,
                Margin = new Thickness(0, 0, 8, 0),
                FontSize = 16,
                Style = (Style)FindResource("PrintButtonStyle")

                //Margin = new Thickness(0, 0, 8, 0),
                //Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                //Foreground = Brushes.White,
                //BorderThickness = new Thickness(0),
                //FontSize = 12,
                //FontWeight = FontWeights.Bold
            };

            // Cancel Button  
            Button cancelButton = new Button
            {
                Content = "ยกเลิก",
                Width = 80,
                Height = 36,
                FontSize = 16,
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Style = (Style)FindResource("PrintButtonStyle")


                //Foreground = Brushes.White,
                //BorderThickness = new Thickness(0),
                //FontSize = 12,
                //FontWeight = FontWeights.Bold
            };

            // Add button click events
            printButton.Click += (sender, e) =>
            {
                e.Handled = true; // Prevent card toggle
                Print(prescription);


            };

            cancelButton.Click += (sender, e) =>
            {
                e.Handled = true; // Prevent card toggle
                CancelPrescription(prescription);
            };

            //SearchTextBox.KeyDown += (sender, e) =>
            //{
            //    if (e.Key == Key.Enter)
            //    {

            //        PrintPrescription(prescription);
            //    }
            //};


            buttonPanel.Children.Add(printButton);
            buttonPanel.Children.Add(cancelButton);

            packageAndButtonGrid.Children.Add(packageSummary);
            packageAndButtonGrid.Children.Add(buttonPanel);

            // Medicine Details (Initially Hidden)
            StackPanel medicinePanel = new StackPanel
            {
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Grid.SetRow(medicinePanel, 5);
            Grid.SetColumn(medicinePanel, 0);

            foreach (var package in prescription.Package)
            {
                System.Windows.Controls.Border packageBorder = new System.Windows.Controls.Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 4, 0, 0)
                };

                StackPanel packagePanel = new StackPanel();

                TextBlock packageName = new TextBlock
                {
                    Text = $"{package.OrderItemName} (รหัส: {package.OrderItemCode})",
                    Style = (Style)FindResource("BodyTextStyle"),
                    FontWeight = FontWeights.SemiBold
                };

                TextBlock packageQty = new TextBlock
                {
                    Text = $"จำนวน: {package.OrderQty} หน่วย",
                    Style = (Style)FindResource("SecondaryTextStyle"),
                    Margin = new Thickness(0, 2, 0, 0)
                };

                packagePanel.Children.Add(packageName);
                packagePanel.Children.Add(packageQty);
                packageBorder.Child = packagePanel;
                medicinePanel.Children.Add(packageBorder);
            }

            // Add click event to toggle details (only on package summary area)
            packageSummary.MouseLeftButtonUp += (sender, e) =>
            {
                e.Handled = true;
                ToggleCardDetails(medicinePanel, expandIcon);
            };
            packageSummary.TouchUp += (sender, e) =>
            {
                e.Handled = true;
                ToggleCardDetails(medicinePanel, expandIcon);
            };

            // Make package summary area look clickable
            packageSummary.Cursor = System.Windows.Input.Cursors.Hand;

            // Remove card-wide click events since we now have buttons
            cardBorder.Cursor = System.Windows.Input.Cursors.Arrow;

            // Add hover effect
            cardBorder.MouseEnter += (sender, e) =>
            {
                cardBorder.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            };

            cardBorder.MouseLeave += (sender, e) =>
            {
                cardBorder.Background = Brushes.White;
            };

            // Add all elements to main grid
            mainGrid.Children.Add(patientPanel);
            mainGrid.Children.Add(prescriptionGrid);
            mainGrid.Children.Add(packageAndButtonGrid);
            mainGrid.Children.Add(medicinePanel);

            cardBorder.Child = mainGrid;
            PrescriptionPanel.Children.Add(cardBorder);
        }

        public void CreatePrescriptionCardReturn(Prescription prescription)
        {
            System.Windows.Controls.Border cardBorder = new System.Windows.Controls.Border();
            cardBorder.Style = (Style)FindResource("CardStyle");
            cardBorder.Cursor = System.Windows.Input.Cursors.Hand;
            cardBorder.Margin = new Thickness(0, 0, 0, 12);

            Grid mainGrid = new Grid();

            // Column definitions
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Row definitions
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Patient info
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Pixel) }); // Spacing
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Prescription info
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Pixel) }); // Spacing
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Package count & buttons
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Medicine details (collapsible)

            // Patient Info
            StackPanel patientPanel = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetRow(patientPanel, 0);
            Grid.SetColumn(patientPanel, 0);

            // Status badge
            System.Windows.Controls.Border statusBorder = new System.Windows.Controls.Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4)
            };

            // Set color based on status
            //if (prescription.Status == "รอจัด")
            //{
            //    statusBorder.Background = (Brush)FindResource("Warning");
            //}
            //else if (prescription.Status == "เสร็จแล้ว")
            //{
            //    statusBorder.Background = (Brush)FindResource("Success");
            //}

            TextBlock statusText = new TextBlock
            {
                Text = prescription.Status,
                Foreground = Brushes.White,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Height = 17
            };
            statusBorder.Child = statusText;

            TextBlock patientName = new TextBlock
            {
                Text = prescription.PatientName,
                Style = (Style)FindResource("SubHeaderTextStyle"),
                Margin = new Thickness(12, 0, 0, 0)
            };

            patientPanel.Children.Add(statusBorder);
            patientPanel.Children.Add(patientName);

            // Prescription Info
            Grid prescriptionGrid = new Grid();
            Grid.SetRow(prescriptionGrid, 2);
            Grid.SetColumn(prescriptionGrid, 0);

            prescriptionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            prescriptionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel leftInfo = new StackPanel();
            Grid.SetColumn(leftInfo, 0);

            leftInfo.Children.Add(new TextBlock
            {
                Text = $"เลขใบสั่งยา: {prescription.PrescriptionNo}",
                Style = (Style)FindResource("BodyTextStyle")
            });
            leftInfo.Children.Add(new TextBlock
            {
                Text = $"HN: {prescription.HN}",
                Style = (Style)FindResource("SecondaryTextStyle")
            });
            leftInfo.Children.Add(new TextBlock
            {
                Text = $"AN: {prescription.AN}",
                Style = (Style)FindResource("SecondaryTextStyle")
            });

            StackPanel rightInfo = new StackPanel();
            Grid.SetColumn(rightInfo, 1);

            rightInfo.Children.Add(new TextBlock
            {
                Text = $"หอผู้ป่วย: {prescription.Ward}",
                Style = (Style)FindResource("BodyTextStyle")
            });
            rightInfo.Children.Add(new TextBlock
            {
                Text = $"เตียง: {prescription.Bed}",
                Style = (Style)FindResource("SecondaryTextStyle")
            });

            prescriptionGrid.Children.Add(leftInfo);
            prescriptionGrid.Children.Add(rightInfo);

            // Package Count Summary and Action Buttons
            Grid packageAndButtonGrid = new Grid();
            Grid.SetRow(packageAndButtonGrid, 4);
            Grid.SetColumn(packageAndButtonGrid, 0);
            Grid.SetColumnSpan(packageAndButtonGrid, 2);

            packageAndButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            packageAndButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel packageSummary = new StackPanel { Orientation = Orientation.Horizontal };
            Grid.SetColumn(packageSummary, 0);

            TextBlock packageCountText = new TextBlock
            {
                Text = $"รายการยา: {prescription.Package.Count} รายการ",
                Style = (Style)FindResource("BodyTextStyle"),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock expandIcon = new TextBlock
            {
                Text = "▼",
                FontSize = 30,
                Margin = new Thickness(8, 0, 0, 0),
                Foreground = (Brush)FindResource("TextSecondary"),
                Name = "ExpandIcon",
                VerticalAlignment = VerticalAlignment.Center
            };

            packageSummary.Children.Add(packageCountText);
            packageSummary.Children.Add(expandIcon);

            // Action Buttons Panel
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(buttonPanel, 1);

            // Print Button
            Button printButton = new Button
            {
                Content = "จัดยา",
                Width = 80,
                Height = 36,
                Margin = new Thickness(0, 0, 8, 0),
                FontSize = 16,
                Style = (Style)FindResource("PrintButtonStyle")

                //Margin = new Thickness(0, 0, 8, 0),
                //Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                //Foreground = Brushes.White,
                //BorderThickness = new Thickness(0),
                //FontSize = 12,
                //FontWeight = FontWeights.Bold
            };

            // Cancel Button  
            Button cancelButton = new Button
            {
                Content = "ยกเลิก",
                Width = 80,
                Height = 36,
                FontSize = 16,
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                Style = (Style)FindResource("PrintButtonStyle")


                //Foreground = Brushes.White,
                //BorderThickness = new Thickness(0),
                //FontSize = 12,
                //FontWeight = FontWeights.Bold
            };

            // Add button click events
            printButton.Click += (sender, e) =>
            {
                e.Handled = true; // Prevent card toggle
                Print(prescription);
            };

            cancelButton.Click += (sender, e) =>
            {
                e.Handled = true; // Prevent card toggle
                CancelPrescription(prescription);
            };

            buttonPanel.Children.Add(printButton);
            buttonPanel.Children.Add(cancelButton);

            packageAndButtonGrid.Children.Add(packageSummary);
            packageAndButtonGrid.Children.Add(buttonPanel);

            // Medicine Details (Initially Hidden)
            StackPanel medicinePanel = new StackPanel
            {
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Grid.SetRow(medicinePanel, 5);
            Grid.SetColumn(medicinePanel, 0);

            foreach (var package in prescription.Package)
            {
                System.Windows.Controls.Border packageBorder = new System.Windows.Controls.Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 4, 0, 0)
                };

                StackPanel packagePanel = new StackPanel();

                TextBlock packageName = new TextBlock
                {
                    Text = $"{package.OrderItemName} (รหัส: {package.OrderItemCode})",
                    Style = (Style)FindResource("BodyTextStyle"),
                    FontWeight = FontWeights.SemiBold
                };

                TextBlock packageQty = new TextBlock
                {
                    Text = $"จำนวน: {package.OrderQty} หน่วย",
                    Style = (Style)FindResource("SecondaryTextStyle"),
                    Margin = new Thickness(0, 2, 0, 0)
                };

                packagePanel.Children.Add(packageName);
                packagePanel.Children.Add(packageQty);
                packageBorder.Child = packagePanel;
                medicinePanel.Children.Add(packageBorder);
            }

            // Add click event to toggle details (only on package summary area)
            packageSummary.MouseLeftButtonUp += (sender, e) =>
            {
                e.Handled = true;
                ToggleCardDetails(medicinePanel, expandIcon);
            };
            packageSummary.TouchUp += (sender, e) =>
            {
                e.Handled = true;
                ToggleCardDetails(medicinePanel, expandIcon);
            };

            // Make package summary area look clickable
            packageSummary.Cursor = System.Windows.Input.Cursors.Hand;

            // Remove card-wide click events since we now have buttons
            cardBorder.Cursor = System.Windows.Input.Cursors.Arrow;

            // Add hover effect
            cardBorder.MouseEnter += (sender, e) =>
            {
                cardBorder.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            };

            cardBorder.MouseLeave += (sender, e) =>
            {
                cardBorder.Background = Brushes.White;
            };

            // Add all elements to main grid
            mainGrid.Children.Add(patientPanel);
            mainGrid.Children.Add(prescriptionGrid);
            mainGrid.Children.Add(packageAndButtonGrid);
            mainGrid.Children.Add(medicinePanel);

            cardBorder.Child = mainGrid;
            PrescriptionPanel.Children.Add(cardBorder);
        }

        private void ToggleCardDetails(StackPanel medicinePanel, TextBlock expandIcon)
        {
            if (medicinePanel.Visibility == Visibility.Collapsed)
            {
                medicinePanel.Visibility = Visibility.Visible;
                expandIcon.Text = "▲";

                // Animate expand with smooth effect
                var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
                };
                medicinePanel.BeginAnimation(UIElement.OpacityProperty, animation);
            }
            else
            {
                medicinePanel.Visibility = Visibility.Collapsed;
                expandIcon.Text = "▼";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            searchDebounceTimer.Stop();
            _ = LoadDataAsync();
            SearchTextBox.Clear();

            // Return focus to SearchTextBox
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
                _focusTimer.Start();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Debounce search to avoid excessive updates
            //SearchTextBox.cl
            searchDebounceTimer.Stop();
            searchDebounceTimer.Start();
        }

        private void SearchDebounceTimer_Tick(object sender, EventArgs e)
        {
            searchDebounceTimer.Stop();
            PerformSearch();
        }

        private void PerformSearch()
        {
            string searchText = SearchTextBox.Text?.ToLower()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredPrescriptions = allPrescriptions.ToList();
            }
            else
            {
                // Optimize: Use parallel search for large datasets
                filteredPrescriptions = allPrescriptions
                    .AsParallel()
                    .Where(p =>
                        p.PatientName.ToLower().Contains(searchText) ||
                        p.HN.ToLower().Contains(searchText) ||
                        p.PrescriptionNo.ToLower().Contains(searchText) ||
                        p.Ward.ToLower().Contains(searchText)
                    )
                    .ToList();
            }

            DisplayPrescriptions();
            UpdateStatistics();
        }

        private void PrintPrescription(Prescription prescription)
        {

            clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescription.PrescriptionNo.ToString());
            if (clsvariable.dt_Prescr.Rows.Count > 0)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(prescription, new JsonSerializerOptions { WriteIndented = true });

                //var popup = new PrescriptionPopup(json);

                //popup.ShowDialog();
                //popup.Topmost = true;
            }
            else
            {
                MessageBox.Show($" ไม่พบใบสั่งยาหมายเลข: {prescription.PrescriptionNo}\nผู้ป่วย: {prescription.PatientName}", "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Error);


            }
        }
        private void Print(Prescription prescription)
        {
            timerRefresh.Stop();
            
            // Use cached data if available
            if (clsvariable.dt_Prescr == null || clsvariable.dt_Prescr.Rows.Count == 0)
            {
                clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescription.PrescriptionNo.ToString());
            }
            
            if (clsvariable.dt_Prescr.Rows.Count > 0)
            {
                // ล้างค่า SearchTextBox หลังจากสแกนสำเร็จ
                SearchTextBox.Clear();
                
                // Show prescription detail in overlay instead of popup
                ShowPrescriptionDetail(prescription);

                string orderitemcode = "";
                string qty = "";
                for (int i = 0; i < prescription.Package.Count; i++)
                {
                    orderitemcode = prescription.Package[i].OrderItemCode.ToString();
                    qty = prescription.Package[i].OrderQty.ToString();

                    if (orderitemcode != "")
                    {
                        ShowLed(orderitemcode, qty);
                        
                    }
                }

                clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescription.PrescriptionNo.ToString());
            }
            else
            {
                MessageBox.Show($" ไม่พบใบสั่งยาหมายเลข: {prescription.PrescriptionNo}\nผู้ป่วย: {prescription.PatientName}", 
                    "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("KeyDown called");
            try
            {
                string prescrip = "";                
                prescrip = SearchTextBox.Text.Trim();
                if(prescrip != "")
                {
                    clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescrip);
                    if (clsvariable.dt_Prescr.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(SearchTextBox.Text))
                        {
                            Prescription presc = new Prescription();
                            presc = DisplayPrescriptionsScanbarCode();
                            Print(presc);
                            // SearchTextBox จะถูกล้างใน Print() method แล้ว
                        }
                    }
                    else
                    {
                        MessageBox.Show($" ไม่พบใบสั่งยาหมายเลข: {prescrip}", "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Error);
                        // ล้างค่าถ้าไม่พบข้อมูล
                        SearchTextBox.Clear();
                    }
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show(" Search TextBox : " + ex.Message);
                SearchTextBox.Clear();
            }
        }
        //private void ScanTimer_Tick(object sender, EventArgs e)
        //{
        //    //// Auto-verify after brief pause in scanning
        //    scanTimer.Stop();
        //    if (!string.IsNullOrEmpty(scannedBarcode))
        //    {
        //        VerifyDispenser(scannedBarcode, "");
        //        scannedBarcode = "";
        //        //LoadSampleData(jsonString);

        //    }
        //    return;
        //}


        private void Refresh_Tick(object sender, EventArgs e)
        {
            //scanTimer.Stop();
            
            // โหลดข้อมูลใหม่จาก database
            DataTable dtPrescriptions = _query.GetPrescription(clsvariable.shelfzone);
            
            if (dtPrescriptions == null || dtPrescriptions.Rows.Count == 0)
            {
                // ถ้าไม่มีข้อมูลใน database ให้ล้างทั้งหมด
                allPrescriptions.Clear();
                filteredPrescriptions.Clear();
                prescriptionCache.Clear();
                DisplayPrescriptions();
                UpdateStatistics();
                return;
            }

            // แปลง DataTable เป็น List<Prescription>
            var prescriptions = dtPrescriptions.AsEnumerable()
                .GroupBy(r => r["prescriptionno"].ToString())
                .Select(g => new
                {
                    prescriptionno = g.Key,
                    hn = g.First()["hn"].ToString(),
                    an = g.First()["an"].ToString(),
                    patientname = g.First()["patientname"].ToString(),
                    ward = g.First()["ward"].ToString(),
                    bed = g.First()["bed"].ToString(),
                    status = "รอจัด",
                    package = g.Select(r => new
                    {
                        orderitemcode = r["orderitemcode"].ToString(),
                        orderitemname = r["orderitemname"].ToString(),
                        orderqty = Convert.ToInt32(r["orderqty"]),
                        addr = r["addr"].ToString(),
                        id = r["position_id"].ToString(),
                        location = r["shelfname"].ToString()
                    }).ToList()
                })
                .ToList();

            string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.None);
            var newPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);

            // สร้าง HashSet ของ PrescriptionNo ใหม่
            var newPrescriptionNos = new HashSet<string>(newPrescriptions.Select(p => p.PrescriptionNo));
            
            // 1. ลบข้อมูลที่หายไปจาก database (ไม่มีใน newPrescriptions)
            var prescriptionsToRemove = allPrescriptions
                .Where(p => !newPrescriptionNos.Contains(p.PrescriptionNo))
                .ToList();
            
            foreach (var prescription in prescriptionsToRemove)
            {
                allPrescriptions.Remove(prescription);
                prescriptionCache.Remove(prescription.PrescriptionNo);
            }

            // 2. เพิ่มเฉพาะข้อมูลใหม่ที่ไม่ซ้ำกับที่มีอยู่
            foreach (var newPrescription in newPrescriptions)
            {
                if (!prescriptionCache.ContainsKey(newPrescription.PrescriptionNo))
                {
                    allPrescriptions.Add(newPrescription);
                    prescriptionCache[newPrescription.PrescriptionNo] = newPrescription;
                }
            }

            // อัพเดท filteredPrescriptions และแสดงผล
            string searchText = SearchTextBox.Text?.ToLower()?.Trim() ?? "";
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredPrescriptions = allPrescriptions.ToList();
            }
            else
            {
                filteredPrescriptions = allPrescriptions
                    .Where(p =>
                        p.PatientName.ToLower().Contains(searchText) ||
                        p.HN.ToLower().Contains(searchText) ||
                        p.PrescriptionNo.ToLower().Contains(searchText) ||
                        p.Ward.ToLower().Contains(searchText)
                    )
                    .ToList();
            }

            DisplayPrescriptions();
            UpdateStatistics();
            
            string datetimeNow = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var main = Application.Current.MainWindow as MainWindow;
        }

      

        private void VerifyDispenser(string code, string password)
        {
            try
            {

                string prescrip = "";
                prescrip = SearchTextBox.Text.Trim();
                clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescrip);
                if (clsvariable.dt_Prescr.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(SearchTextBox.Text))
                    {
                        //MessageBox.Show("คุณกด Enter แล้ว!");
                        Prescription presc = new Prescription();

                        presc = DisplayPrescriptionsScanbarCode();
                        Print(presc);
                    }
                    return;
                }
                else
                {
                    //MessageBox.Show($" ไม่พบใบสั่งยาหมายเลข: {prescrip}", "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(" Search TextBox : " + ex.Message);
            }
        }


        private void CancelPrescription(Prescription prescription)
        {
            var result = MessageBox.Show($"ต้องการยกเลิกใบสั่งยาหมายเลข: {prescription.PrescriptionNo}\nผู้ป่วย: {prescription.PatientName} หรือไม่?",
                                       "Cancel Prescription", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Update prescription status or remove from list
                prescription.Status = "ยกเลิก";

                // Refresh the display
                DisplayPrescriptions();
                UpdateStatistics();

                MessageBox.Show("ยกเลิกใบสั่งยาเรียบร้อยแล้ว", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void LoadReport(DataTable dt)
        {
            ReportDocument report = new ReportDocument();
            report.Load(clsvariable.crp_report);
            report.SetDataSource(dt);
            report.PrintOptions.PrinterName = clsvariable.printname;
            report.PrintToPrinter(1, false, 0, 0);
            report.Close();
            report.Dispose();
        }
        public void ShowLed(string orderitem, string qty)
        {
            DataTable dt_stock = new DataTable();
            bool result = false;
            dt_stock = _query.GetStockByCode(orderitem);
            int order_qty = Convert.ToInt32(qty);
            if (dt_stock.Rows.Count > 0)
            {
                string Lot = dt_stock.Rows[0]["LotNo"].ToString();
                string Exp = dt_stock.Rows[0]["Exp"].ToString();
                int stock = Convert.ToInt32(dt_stock.Rows[0]["In_Qty"].ToString())- order_qty;
                int R = Convert.ToInt32(clsvariable.RGD_dispense[0].ToString());
                int G = Convert.ToInt32(clsvariable.RGD_dispense[1].ToString());
                int B = Convert.ToInt32(clsvariable.RGD_dispense[2].ToString());
                int position_id = 0;
                if (dt_stock.Rows[0]["position_id"].ToString() != "")
                {
                    position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                }
                
                int addr = 0;
                if (dt_stock.Rows[0]["addr"].ToString() != "")
                {
                    addr = Convert.ToInt32(dt_stock.Rows[0]["addr"].ToString());
                }
                    

                //clsvariable.Instance.SerialCan.SetLED(1, position_id,R, G, B);
                clsvariable.Instance.SerialCan.Order(addr, position_id, qty, " ", Lot, Exp, stock.ToString(), R, G, B);

                //_serial.Button_event(addr.ToString(), position_id.ToString(), qty);

            }
            else
            {
                MessageBox.Show($"ไม่พบข้อมูลสต๊อกของ {orderitem} กรุณาตรวจสอบ stock ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void InvenStock(string orderitem, string qty)
        {
            DataTable dt_stock = new DataTable();
            bool result = false;
            dt_stock = _query.GetStockByCode(orderitem);
            int order_qty = Convert.ToInt32(qty);

            DataTable db_print = new DataTable();
            db_print.Columns.Add("prescriptionno", typeof(String));
            db_print.Columns.Add("orderitemname", typeof(String));
            db_print.Columns.Add("orderqty", typeof(String));
            db_print.Columns.Add("patientname", typeof(String));
            db_print.Columns.Add("hn", typeof(String));
            db_print.Columns.Add("freetext1", typeof(String));
            db_print.Columns.Add("freetext2", typeof(String));
            db_print.Columns.Add("freetext3", typeof(String));
            db_print.Columns.Add("freetext4", typeof(String));
            db_print.TableName = "db_print";
            if (clsvariable.dt_Prescr.Rows.Count > 0)
            {
                //if (dt_stock.Rows.Count == 1)
                //{
                //    foreach (DataRow row in dt_stock.Rows)
                //    {
                //        string prescriptionno = clsvariable.dt_Prescr.Rows[0]["prescriptionno"].ToString();
                //        string seq = clsvariable.dt_Prescr.Rows[0]["seq"].ToString();
                //        string orderitemcode = clsvariable.dt_Prescr.Rows[0]["orderitemcode"].ToString();
                //        int stock_qty = Convert.ToInt32(row["In_Qty"].ToString());
                //        int new_qty = stock_qty - order_qty;
                //        string lot = row["LotNo"].ToString();
                //        string exp = row["Exp"].ToString();

                //        result = _query.UpdateDisStock(new_qty.ToString(), orderitem, lot, exp);
                //        if (result)
                //        {
                //            Debug.WriteLine($"Update stock {orderitem} new qty: {new_qty}");
                //            result = _query.UpdateJob("USER", prescriptionno, seq, orderitemcode);
                //        }
                //        else
                //        {
                //            Debug.WriteLine($"Update stock {orderitem} error");
                //        }
                //    }
                //}
                //else if (dt_stock.Rows.Count > 1)
                //{
                //    foreach (DataRow row in dt_stock.Rows)
                //    {
                //        string prescriptionno = clsvariable.dt_Prescr.Rows[0]["prescriptionno"].ToString();
                //        string seq = clsvariable.dt_Prescr.Rows[0]["seq"].ToString();
                //        string orderitemcode = clsvariable.dt_Prescr.Rows[0]["orderitemcode"].ToString();
                //        int stock_qty = Convert.ToInt32(row["In_Qty"].ToString());
                //        int new_qty = stock_qty - order_qty;
                //        string lot = row["LotNo"].ToString();
                //        string exp = row["Exp"].ToString();
                //        result = _query.UpdateDisStock(new_qty.ToString(), orderitem, lot, exp);
                //        if (result)
                //        {
                //            Debug.WriteLine($"Update stock {orderitem} new qty: {new_qty}");
                //            result = _query.UpdateJob("USER", prescriptionno, seq, orderitemcode);
                //        }
                //        else
                //        {
                //            Debug.WriteLine($"Update stock {orderitem} error");
                //        }

                //        if (new_qty == 0)
                //        {
                //            break;
                //        }
                //        else
                //        {
                //            continue;
                //        }
                //    }
                //}
                //else
                //{
                //    MessageBox.Show($"ไม่พบข้อมูลสต๊อกของ {orderitem} กรุณาตรวจสอบ stock ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}


                foreach (DataRow row in clsvariable.dt_Prescr.Rows)
                {
                    DataRow r = db_print.Rows.Add();
                    r["prescriptionno"] = row["prescriptionno"].ToString();
                    r["orderitemname"] = row["orderitemname"].ToString();
                    r["orderqty"] = row["orderqty"].ToString();
                    r["patientname"] = row["patientname"].ToString();
                    r["hn"] = row["hn"].ToString();
                    r["freetext1"] = row["freetext1"].ToString();
                    r["freetext2"] = row["freetext2"].ToString();
                    r["freetext3"] = row["freetext3"].ToString();
                    r["freetext4"] = row["freetext4"].ToString();
                }

                if (db_print.Rows.Count > 0)
                {
                    MessageBox.Show(clsvariable.crp_report.ToString() + "  " + cls.clsvariable.printname.ToString());
                    //LoadReport(db_print);
                }
            }

        }


        private void Rx_OnTriggerReceived(string data)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {



                    var incoming = JsonConvert.DeserializeObject<List<Prescription>>(data);

                    foreach (var p in incoming)
                    {

                        var existing = allPrescriptions.FirstOrDefault(x => x.PrescriptionNo == p.PrescriptionNo);

                        if (existing != null)
                        {

                            existing.HN = p.HN;
                            existing.PatientName = p.PatientName;
                            existing.Ward = p.Ward;
                            existing.Bed = p.Bed;
                            existing.Status = p.Status;

                            foreach (var pkg in p.Package)
                            {
                                var existPkg = existing.Package.FirstOrDefault(x => x.OrderItemCode == pkg.OrderItemCode);
                                if (existPkg != null)
                                {
                                    // อัปเดท orderqty
                                    existPkg.OrderQty = pkg.OrderQty;
                                }
                                else
                                {
                                    existing.Package.Add(pkg);
                                }
                            }
                        }
                        else
                        {
                            // เพิ่ม prescription ใหม่
                            allPrescriptions.Add(p);

                        }
                    }
                    filteredPrescriptions = allPrescriptions.ToList();
                    DisplayPrescriptions();
                    UpdateStatistics();
                    // Parse JSON array
                    //var arr = Newtonsoft.Json.Linq.JArray.Parse(data);


                    //allPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(data);

                    //filteredPrescriptions = allPrescriptions.ToList();
                    //DisplayPrescriptions();
                    //UpdateStatistics();
                    //foreach (var item in arr)
                    //{
                    //    string prescriptionno = item["prescriptionno"]?.ToString();
                    //    string hn = item["hn"]?.ToString();
                    //    string patientname = item["patientname"]?.ToString();
                    //    string ward = item["ward"]?.ToString();
                    //    string bed = item["bed"]?.ToString();
                    //    string status = item["status"]?.ToString();

                    //    //MessageBox.Show(
                    //    //    $"HN: {hn}\nPatient: {patientname}\nWard: {ward}\nBed: {bed}\nStatus: {status}\nPrescription: {prescriptionno}"
                    //    //);

                    //    //  package
                    //    foreach (var pkg in item["package"])
                    //    {
                    //        string code = pkg["orderitemcode"]?.ToString();
                    //        string name = pkg["orderitemname"]?.ToString();
                    //        string qty = pkg["orderqty"]?.ToString();

                    //        //MessageBox.Show($"Package: {code} - {name} x{qty}");
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show("JSON parse error: " + ex.Message);
                }
            });
        }


        // PrescriptionPopup Methods
        private void ShowPrescriptionDetail(Prescription prescription)
        {
            if (prescription == null) return;
            
            var prescriptions = new[]
            {
                new
                {
                    prescriptionno = prescription.PrescriptionNo,
                    hn = prescription.HN,
                    an = prescription.AN,
                    patientname = prescription.PatientName,
                    ward = prescription.Ward,
                    bed = prescription.Bed,
                    status = prescription.Status,
                    package = prescription.Package.Select(p => new
                    {
                        orderitemcode = p.OrderItemCode,
                        orderitemname = p.OrderItemName,
                        orderqty = p.OrderQty,
                        addr = "",
                        id = "",
                        location = p.Location
                    }).ToList()
                }
            }.ToList();
            
            jsonString = JsonConvert.SerializeObject(prescriptions, Formatting.None);
            LoadPrescriptionDetail(jsonString);
        }
        
        private void LoadPrescriptionDetail(string _jsonString)
        {
            jsonString = _jsonString;
            if (jsonString != "")
            {
                LoadSampleData(jsonString);               
                // Show overlay
                Dispatcher.Invoke(() =>
                {
                    if (PrescriptionDetailOverlay != null)
                    {
                        PrescriptionDetailOverlay.Visibility = Visibility.Visible;
                    }
                });
            }
        }
        
        private void ClosePrescriptionDetail()
        {
            if (PrescriptionDetailOverlay != null)
            {
                PrescriptionDetailOverlay.Visibility = Visibility.Collapsed;
            }
            selectedItems.Clear();
            prescriptionData = null;
            isVerified = false;
            timerBtn.Stop();
            
            // Return focus to SearchTextBox when closing detail
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTextBox.Clear(); // ล้างค่าเมื่อปิด overlay
                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
                _focusTimer.Start();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
        
        private void ClosePrescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            ClosePrescriptionDetail();
            LoadPrescriptions();
        }
        
        private void LoadSampleData(string jsonString)
        {
            LoadData(jsonString);
        }
        
        public void LoadData(string jsonData)
        {
            try
            {
                var prescriptions = JsonConvert.DeserializeObject<List<PrescriptionData>>(jsonData);
                if (prescriptions != null && prescriptions.Count > 0)
                {
                    prescriptionData = prescriptions[0];
                    
                    Dispatcher.Invoke(() =>
                    {
                        if (txtPrescriptionNo != null) txtPrescriptionNo.Text = prescriptionData.PrescriptionNo;
                        if (txtHN != null) txtHN.Text = prescriptionData.HN;
                        if (txtAN != null) txtAN.Text = prescriptionData.AN;
                        if (txtPatientName != null) txtPatientName.Text = prescriptionData.PatientName;
                        if (txtWard != null) txtWard.Text = prescriptionData.Ward;
                        if (txtBed != null) txtBed.Text = prescriptionData.Bed;
                        if (PackageItemsControl != null) PackageItemsControl.ItemsSource = prescriptionData.Package;
                        if (txtTotalItems != null) txtTotalItems.Text = prescriptionData.Package.Count.ToString();
                        if (txtSelectedItems != null) txtSelectedItems.Text = "0";
                        
                        clsvariable.PackItem = prescriptionData.Package.Count;
                        clsvariable.CountItem = 0;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดข้อมูล: {ex.Message}");
            }
        }
        
        public void PackageCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (PackageItemsControl != null)
            {

                // หา Border ที่ถูกคลิก (อาจเป็น child element)
                var element = e.OriginalSource as FrameworkElement;
                Border cardBorder = null;
                
                // ค้นหา Border parent ที่มี DataContext
                while (element != null)
                {
                    if (element is Border border && border.DataContext is PackageItem)
                    {
                        cardBorder = border;
                        break;
                    }
                    element = element.Parent as FrameworkElement;
                }
                
                if (cardBorder == null) return;
                
                var item = cardBorder.DataContext as PackageItem;
                if (item == null) return;
                
                // หา Grid ที่เป็น child ของ Border
                var grid = cardBorder.Child as Grid;
                if (grid == null) return;
                
                // หา CheckMark ใน Grid
                var checkMark = grid.Children.OfType<Border>().FirstOrDefault(b => b.Name == "CheckMark");
                if (checkMark == null) return;
                
                if (selectedItems.Contains(item))
                {
                    selectedItems.Remove(item);
                    AnimateCheckMark(checkMark, cardBorder, false);
                }
                else
                {
                    selectedItems.Add(item);
                    AnimateCheckMark(checkMark, cardBorder, true);
                    //InvenStock(item.OrderItemCode, item.OrderQty.ToString());
                }
                
                UpdateFooter();

                PrintPrescriptionNoLed(item);

                //timerBtn.Start();
            }
            
            Cradclick = true;
        }
        private void PrintPrescriptionNoLed(object prescription)
        {
            //timerBtn.Stop();
            string orderitemcode = ((dynamic)prescription).OrderItemCode;
            int qty = ((dynamic)prescription).OrderQty;
            string addr = ((dynamic)prescription).Addr;
            string id = ((dynamic)prescription).id;

            DataTable dt_stock = new DataTable();
            dt_stock = _query.GetLedStockByCode_NoLED(orderitemcode, clsvariable.shelfzone);
            if (dt_stock.Rows.Count != 0)
            {
                //InvenStock(dt_stock.Rows[0]["drugCode"].ToString(), qty.ToString());
                DataTable dt_stock_ = new DataTable();
                dt_stock_ = _query.GetLedStockByAddr(dt_stock.Rows[0]["addr"].ToString(), dt_stock.Rows[0]["position_id"].ToString(), clsvariable.shelfzone);
                InvenStock_Addr(dt_stock.Rows[0]["addr"].ToString(), dt_stock.Rows[0]["position_id"].ToString(), qty.ToString());
            }
            else
            {
                clsvariable.StrU = "";
            }

        }
        public void InvenStock_Addr(string Row, string Addr, string qty)
        {
            try
            {
                DataTable dt_stockAddr = new DataTable();
                DataTable dt_stock = new DataTable();
                DataTable dt_regist_flag = new DataTable();
                string orderitem = "";
                bool result = false;
                string regist_flag = "";
                //MessageBox.Show(Row + "|" + Addr + "|" + clsvariable.shelfzone);
                dt_stockAddr = _query.GetLedStockByAddr(Row, Addr, clsvariable.shelfzone);
                if (dt_stockAddr.Rows.Count > 0)
                {
                    orderitem = dt_stockAddr.Rows[0]["drugCode"].ToString();

                    dt_stock = _query.GetStockByCode(orderitem);
                    int order_qty = Convert.ToInt32(qty);

                    DataTable db_print = new DataTable();
                    db_print.Columns.Add("prescriptionno", typeof(String));
                    db_print.Columns.Add("orderitemname", typeof(String));
                    db_print.Columns.Add("orderqty", typeof(String));
                    db_print.Columns.Add("patientname", typeof(String));
                    db_print.Columns.Add("hn", typeof(String));
                    db_print.Columns.Add("freetext1", typeof(String));
                    db_print.Columns.Add("freetext2", typeof(String));
                    db_print.Columns.Add("freetext3", typeof(String));
                    db_print.Columns.Add("freetext4", typeof(String));
                    db_print.Columns.Add("ward", typeof(String));
                    db_print.Columns.Add("orderitemnameTH", typeof(String));
                    db_print.Columns.Add("QRcode", typeof(byte[]));
                    db_print.Columns.Add("location", typeof(String));
                    db_print.TableName = "db_print";

                    clsvariable.dt_Prescr = _query.GetPrescriptionByCode(txtPrescriptionNo.Text.Trim());

                    if (clsvariable.dt_Prescr.Rows.Count > 0)
                    {
                        DataTable dt_prescr = new DataTable();
                        DataRow[] rows = clsvariable.dt_Prescr.Select("orderitemcode = '" + orderitem.Replace("'", "''") + "'");

                        if (rows.Length > 0)
                        {
                            dt_prescr = rows.CopyToDataTable();
                        }
                        else
                        {
                            dt_prescr = clsvariable.dt_Prescr.Clone(); // คืน DataTable โครงสร้างเดิมแต่ไม่มีข้อมูล
                        }

                        if (dt_prescr.Rows.Count > 0)
                        {
                            dt_regist_flag = _query.GetRegist_flag(dt_prescr.Rows[0]["hn"].ToString());
                            if (dt_regist_flag.Rows.Count > 0)
                            {
                                regist_flag = dt_regist_flag.Rows[0]["regist_flag"].ToString();
                            }
                            string prescriptionno = dt_prescr.Rows[0]["prescriptionno"].ToString();
                            string seq = dt_prescr.Rows[0]["seq"].ToString();
                            if (dt_stock.Rows.Count == 0)
                            {
                                MessageBox.Show($"ไม่พบข้อมูลสต๊อกของ {orderitem} กรุณาตรวจสอบ stock ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                int remaining_qty = order_qty; // จำนวนที่ต้องตัดออก
                                int index = 0;
                                //timerBtn.Stop();

                                while (index < dt_stock.Rows.Count && remaining_qty > 0)
                                {
                                    DataRow row = dt_stock.Rows[index];
                                    int stock_qty = Convert.ToInt32(row["In_Qty"].ToString());
                                    string lot = row["LotNo"].ToString();
                                    string exp = row["Exp"].ToString();

                                    int new_qty = stock_qty - remaining_qty;

                                    if (new_qty >= 0) // ใช้ lot นี้พอหรือเหลือ
                                    {
                                        result = _query.UpdateDisStock(new_qty.ToString(), orderitem, lot, exp);
                                        if (result) Debug.WriteLine($"Update stock {orderitem} new qty: {new_qty}");

                                        result = _query.InsertLog($"Update stock {orderitem} ตัดสต๊อก : {remaining_qty}", "", clsvariable.shelfzone);

                                        remaining_qty = 0; // ตัดครบแล้ว
                                    }
                                    else // ไม่พอ ต้องใช้ lot ถัดไป
                                    {
                                        result = _query.UpdateDisStock("0", orderitem, lot, exp);
                                        if (result) Debug.WriteLine($"Update stock {orderitem} new qty: 0");
                                        remaining_qty = Math.Abs(new_qty); // ยังเหลือให้ตัดแถวถัดไป

                                        result = _query.InsertLog($"Update stock {orderitem} ตัดสต๊อก : {stock_qty}", "", clsvariable.shelfzone);
                                    }

                                    index++;
                                }
                                //MessageBox.Show(result.ToString());
                                if (result)
                                {
                                    result = _query.UpdateJob(cls.clsvariable.user, prescriptionno, seq, orderitem);
                                    //SyncDrug(orderitem);
                                    LoadStockByCode(orderitem);
                                }
                                else
                                {
                                    //Debug.WriteLine($"Update stock {orderitem} error");
                                    MessageBox.Show($"Update stock {orderitem} error");
                                }

                                foreach (DataRow row in dt_prescr.Rows)
                                {
                                    DataRow r = db_print.Rows.Add();
                                    r["prescriptionno"] = row["prescriptionno"].ToString();
                                    r["orderitemname"] = row["orderitemname"].ToString();
                                    r["orderitemnameTH"] = row["orderitemnameTH"].ToString();
                                    r["orderqty"] = row["orderqty"].ToString().Split('.')[0];
                                    r["patientname"] = row["patientname"].ToString();
                                    r["hn"] = row["hn"].ToString() + "-" + regist_flag;
                                    r["freetext1"] = row["freetext1"].ToString();
                                    r["freetext2"] = row["freetext2"].ToString();
                                    r["freetext3"] = row["freetext3"].ToString();
                                    r["freetext4"] = row["freetext4"].ToString();
                                    r["ward"] = row["wardcode"].ToString() + " " + row["wardname"].ToString();

                                    MemoryStream mss = new MemoryStream();
                                    byte[] bytess = mss.ToArray();
                                    genQr(row["prescriptionno"].ToString()).Save(mss, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    bytess = mss.ToArray();
                                    if (bytess.Length > 0)
                                    {
                                        r["QRcode"] = bytess;
                                    }
                                    else
                                    {
                                        r["QRcode"] = "";
                                    }
                                    r["location"] = row["shelfzone"].ToString() + "-" + row["shelfname"].ToString();
                                }

                                if (clsvariable.print_isenable)
                                {
                                    if (db_print.Rows.Count > 0)
                                    {
                                        //LoadReport(db_print);
                                    }
                                }

                            }

                            //MessageBox.Show($@"{clsvariable.CountItem} = {prescriptionData.Package.Count}");

                            if (clsvariable.CountItem == clsvariable.PackItem)
                            {
                               
                            }
                            else
                            {

                            }

                        }
                    }
                }
                else
                {
                    MessageBox.Show($@" ไม่พบข้อมูลยา");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void AnimateCheckMark(Border checkMark, Border cardBorder, bool show)
        {
            if (show)
            {
                checkMark.Visibility = Visibility.Visible;
                
                var scaleTransform = new ScaleTransform(0, 0, 0.5, 0.5);
                checkMark.RenderTransform = scaleTransform;
                
                var scaleAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
                };
                
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                
                var gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new System.Windows.Point(0, 0);
                gradientBrush.EndPoint = new System.Windows.Point(1, 0);
                gradientBrush.GradientStops.Add(new GradientStop((MediaColor)MediaColorConverter.ConvertFromString("#3b82f6"), 0));
                gradientBrush.GradientStops.Add(new GradientStop((MediaColor)MediaColorConverter.ConvertFromString("#8b5cf6"), 1));
                cardBorder.BorderBrush = gradientBrush;
            }
            else
            {
                var fadeAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                fadeAnimation.Completed += (s, e) => checkMark.Visibility = Visibility.Collapsed;
                checkMark.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
                cardBorder.BorderBrush = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#e2e8f0"));
            }
        }
        
        private void UpdateFooter()
        {
            if (txtSelectedItems != null && prescriptionData != null)
            {
                txtSelectedItems.Text = selectedItems.Count.ToString();
                txtTotalItems.Text = prescriptionData.Package.Count.ToString();
            }
        }
        
        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการยาที่จัดเสร็จแล้ว", "แจ้งเตือน", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            timerBtn.Stop();
            ClosePrescriptionDetail();
            await InitializePageAsync();
        }
        
        public List<PackageItem> GetSelectedItems()
        {
            return selectedItems.ToList();
        }
        
        public static System.Drawing.Image genQr(string txt)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(txt, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            DrawingBitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }
        
        public void LoadStockByCode(string code)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = _query.GetAllLedStockByCode(code, clsvariable.shelfzone);
                string connStr = GD4_LED.Properties.Settings.Default.connectstring;
                if (dt.Rows.Count > 0)
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        conn.Open();


                        string sql = $@" DELETE FROM ms_stock where shelfzone = '{clsvariable.shelfzone}' and orderitemcode = '{code}'; ";

                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }


                        var columns = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

                        string insertCols = string.Join(", ", columns);
                        string insertParams = string.Join(", ", columns.Select(c => "@" + c));

                        string updateCols = string.Join(", ", columns
                                                        .Where(c => c != "orderitemcode")
                                                        .Select(c => $"{c} = VALUES({c})"));

                        sql = $@" INSERT INTO ms_stock ({insertCols}) VALUES ({insertParams}) ON DUPLICATE KEY UPDATE {updateCols}; ";

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

                    //SyncDrug(code);
                }
                else
                {
                    MessageBox.Show("ไม่มีข้อมูลตำแหน่งยาในตู้ LED นี้", "ข้อมูลว่าง",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("LoadStockByCode : " + ex.ToString());
            }

        }

        // ทำลาย Resources เมื่อหน้าถูกปิด
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Cleanup timers
            searchDebounceTimer?.Stop();
            timerRefresh?.Stop();
            _focusTimer?.Stop();
            
            // Clear cache
            prescriptionCache?.Clear();

            // หยุด Animations
            var loadingStoryboard = (Storyboard)this.Resources["LoadingAnimation"];
            loadingStoryboard?.Stop();
        }

        public void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string prescrip = "";
                    prescrip = SearchTextBox.Text.Trim();
                    clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescrip);
                    if (clsvariable.dt_Prescr.Rows.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(SearchTextBox.Text))
                        {
                            Prescription presc = new Prescription();
                            presc = DisplayPrescriptionsScanbarCode();
                            Print(presc);
                            // SearchTextBox จะถูกล้างใน Print() method แล้ว
                        }
                    }
                    else
                    {
                        MessageBox.Show($" ไม่พบใบสั่งยาหมายเลข: {prescrip}", "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Error);
                        // ล้างค่าถ้าไม่พบข้อมูล
                        SearchTextBox.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(" Search TextBox : " + ex.Message);
                    SearchTextBox.Clear();
                }
            }
        }

        private void Page_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // หยุด timer เมื่อมีการคลิก
            _focusTimer.Stop();
            _allowFocusReturn = false;
            
            // เริ่ม timer ใหม่เพื่อคืน focus ภายหลัง
            _focusTimer.Start();
        }

        private void FocusTimer_Tick(object sender, EventArgs e)
        {
            _focusTimer.Stop();
            _allowFocusReturn = true;
            
            // คืน focus กลับมาที่ SearchTextBox ถ้าไม่มี overlay เปิดอยู่
            if (PrescriptionDetailOverlay?.Visibility != Visibility.Visible)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!SearchTextBox.IsFocused)
                    {
                        SearchTextBox.Focus();
                        Keyboard.Focus(SearchTextBox);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }


    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
    
    // PrescriptionData and PackageItem classes
    public class PrescriptionData
    {
        public string PrescriptionNo { get; set; }
        public string HN { get; set; }
        public string AN { get; set; }
        public string PatientName { get; set; }
        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Status { get; set; }
        public string Addr { get; set; }
        public string id { get; set; }
        public List<PackageItem> Package { get; set; }
    }

    public class PackageItem
    {
        public string OrderItemCode { get; set; }
        public string OrderItemName { get; set; }
        public int OrderQty { get; set; }
        public string Location { get; set; }
        public string Addr { get; set; }
        public string id { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PackageItem other)
                return OrderItemCode == other.OrderItemCode;
            return false;
        }

        public override int GetHashCode()
        {
            return OrderItemCode?.GetHashCode() ?? 0;
        }
    }
}
