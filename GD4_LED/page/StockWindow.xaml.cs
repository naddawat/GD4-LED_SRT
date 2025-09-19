using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json; // เพิ่ม using สำหรับ JSON
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows.Media.Animation;

namespace GD4_LED.page
{
    /// <summary>
    /// Interaction logic for StockWindow.xaml
    /// </summary>
    public partial class StockWindow : Page
    {
        public ObservableCollection<DrugStockModel> DrugStocks { get; set; }
        private ObservableCollection<DrugStockModel> AllDrugStocks { get; set; }
        public int TotalCount => DrugStocks?.Count ?? 0;
        //public int LowStockCount => DrugStocks?.Count(x => x.Quantity <= 0) ?? 0; // ปรับเงื่อนไขตามที่ต้องการ
        public int LowStockCount => DrugStocks?.Count(x => x.Quantity < x.min) ?? 0;

        // เพิ่ม property สำหรับ popup data
        public object SelectedDrug { get; set; }
        public int RefillQuantity { get; set; }
        public string RefillLot { get; set; }
        public DateTime? RefillExpiryDate { get; set; } = DateTime.Today.AddMonths(12);
        public string RefillNotes { get; set; }


        cls.clsStock _STK = new cls.clsStock();
        public StockWindow()
        {
            InitializeComponent();
            DataTable dt = new DataTable();
            dt = _STK.GetLedStock();
            string jsonData = JsonConvert.SerializeObject(dt, Formatting.Indented);
            

            AllDrugStocks = JsonConvert.DeserializeObject<ObservableCollection<DrugStockModel>>(jsonData);

            DrugStocks = JsonConvert.DeserializeObject<ObservableCollection<DrugStockModel>>(jsonData);
            this.DataContext = this;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // กรองเมื่อกด Enter หรือทุก Key ก็ได้
            //if (e.Key == Key.Enter || e.Key == Key.Return)
            //{

           
            var textBox = sender as TextBox;
            string keyword = textBox.Text.Trim().ToLower();

            var filtered = AllDrugStocks.Where(x =>
                (x.drugCode != null && x.drugCode.ToLower().Contains(keyword)) ||
                (x.drugName != null && x.drugName.ToLower().Contains(keyword))
            ).ToList();

            DrugStocks.Clear();
            foreach (var item in filtered)
                DrugStocks.Add(item);

            // อัปเดตสรุป
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(LowStockCount));

            if (SearchTextBox.Text.Length > 0)
            {
                SearchPlaceholder.Text = "";
            } else
            {
                SearchPlaceholder.Text = "Med Code/Name";
            }
            //}
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RefillMedicine_Click(object sender, RoutedEventArgs e)
        {
        
            // ดึงข้อมูลยาจาก Button's DataContext
            var button = sender as Button;
            var drugData = button?.DataContext;

            if (drugData != null)
            {
                SelectedDrug = drugData;
                //string a = SelectedDrug.drugCode;
                // รีเซ็ตข้อมูลใน popup
                //RefillQuantity = 0;
                //RefillLot = "";
                //RefillExpiryDate = DateTime.Today.AddMonths(12);
                //RefillNotes = "";

                // แสดง popup
                //RefillPopupOverlay.Visibility = Visibility.Visible;

                // Focus ที่ช่องจำนวน
                //RefillQuantityTextBox.Focus();
            }
            ShowPopup(SelectedDrug);
        }

        //private void CloseRefillPopup_Click(object sender, RoutedEventArgs e)
        //{
        //    // Animation สำหรับซ่อน popup
        //    var fadeOut = new DoubleAnimation
        //    {
        //        From = 1,
        //        To = 0,
        //        Duration = TimeSpan.FromMilliseconds(200)
        //    };

        //    fadeOut.Completed += (s, args) => {
        //        RefillPopupOverlay.Visibility = Visibility.Collapsed;
        //    };

        //    RefillPopupOverlay.BeginAnimation(Grid.OpacityProperty, fadeOut);
        //}

        //private void CancelRefill_Click(object sender, RoutedEventArgs e)
        //{
        //    CloseRefillPopup_Click(sender, e);
        //}

        //private void ConfirmRefill_Click(object sender, RoutedEventArgs e)
        //{
        //    // ตรวจสอบข้อมูลที่กรอก
        //    if (ValidateRefillData())
        //    {
        //        try
        //        {
        //            // บันทึกข้อมูลการเติมยา
        //            SaveRefillData();

        //            // แสดงข้อความสำเร็จ
        //            ShowSuccessMessage();

        //            // ปิด popup
        //            CloseRefillPopup_Click(sender, e);

        //            // รีเฟรชข้อมูล
        //            RefreshDrugStocks();
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด",
        //                           MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        //private bool ValidateRefillData()
        //{
        //    var errors = new List<string>();

        //    // ตรวจสอบจำนวน
        //    if (RefillQuantity <= 0)
        //    {
        //        errors.Add("กรุณาระบุจำนวนที่ต้องการเติม");
        //        RefillQuantityTextBox.Focus();
        //    }

        //    // ตรวจสอบ Lot
        //    if (string.IsNullOrWhiteSpace(RefillLot))
        //    {
        //        errors.Add("กรุณาระบุ Lot Number");
        //        if (errors.Count == 1) RefillLotTextBox.Focus();
        //    }

        //    // ตรวจสอบวันหมดอายุ
        //    if (!RefillExpiryDate.HasValue || RefillExpiryDate <= DateTime.Today)
        //    {
        //        errors.Add("กรุณาระบุวันหมดอายุที่ถูกต้อง");
        //        if (errors.Count == 1) RefillExpiryDatePicker.Focus();
        //    }

        //    // แสดงข้อผิดพลาด
        //    if (errors.Any())
        //    {
        //        string errorMessage = string.Join("\n• ", errors.Prepend("กรุณาแก้ไขข้อมูลดังนี้:"));
        //        MessageBox.Show(errorMessage, "ข้อมูลไม่ครบถ้วน",
        //                       MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return false;
        //    }

        //    return true;
        //}

        private void SaveRefillData()
        {
            // TODO: บันทึกข้อมูลลงฐานข้อมูล
            // ตัวอย่างการบันทึก:

            var refillRecord = new
            {
                DrugCode = ((dynamic)SelectedDrug).drugCode,
                Quantity = RefillQuantity,
                LotNumber = RefillLot,
                ExpiryDate = RefillExpiryDate.Value,
                Notes = RefillNotes,
                RefillDate = DateTime.Now,
                UserId = "CurrentUser" // ใส่ ID ของผู้ใช้ปัจจุบัน
            };

            // บันทึกลงฐานข้อมูล
            // DatabaseService.SaveRefillRecord(refillRecord);

            // อัพเดทสต็อกปัจจุบัน
            // DatabaseService.UpdateDrugStock(drugCode, newQuantity);

            Console.WriteLine($"บันทึกการเติมยา: {refillRecord}");
        }

        private void ShowSuccessMessage()
        {
            string message = $"เติมยาสำเร็จ!\n\n" +
                            $"รหัสยา: {((dynamic)SelectedDrug).drugCode}\n" +
                            $"จำนวนที่เติม: {RefillQuantity:N0} หน่วย\n" +
                            $"Lot: {RefillLot}\n" +
                            $"วันหมดอายุ: {RefillExpiryDate:dd/MM/yyyy}";

            MessageBox.Show(message, "เติมยาสำเร็จ",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshDrugStocks()
        {
            // TODO: รีเฟรชข้อมูลสต็อกยา
            // ViewModel.LoadDrugStocks();
            Console.WriteLine("รีเฟรชข้อมูลสต็อกยา");
        }

        public void ShowPopup(object SelectedDrug)
        {
            // สร้าง Window สำหรับแสดง Popup Page
            Window popupWindow = new Window
            {
                Title = "Popup",
                Width = 800,
                Height = 1400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None, // ซ่อน Title Bar ทั้งหมด
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                AllowsTransparency = true, // จำเป็นต้องใช้เมื่อ WindowStyle=None
                Background = Brushes.Transparent // พื้นหลังโปร่งใส
            };

            List<RefillRecord> refillList = new List<RefillRecord>
            {
                new RefillRecord
                {
                    DrugCode = ((dynamic)SelectedDrug).drugCode,
                    Quantity = RefillQuantity,
                    LotNumber = RefillLot,
                    ExpiryDate = RefillExpiryDate.Value,
                    Notes = RefillNotes,
                    RefillDate = DateTime.Now,
                    UserId = "CurrentUser"
                }
            };

            PopupRefill popupPage = new PopupRefill(refillList);

            // กำหนด Content ของ Window เป็น Popup Page
            popupWindow.Content = popupPage;

            // แสดง Popup
            popupWindow.ShowDialog();
        }

    }
}
