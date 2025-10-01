using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
using GD4_LED.cls;

namespace GD4_LED.page
{
    /// <summary>
    /// Interaction logic for PopupRefill.xaml
    /// </summary>
    public partial class PopupRefill : Page
    {
        public List<RefillRecord> SelectedDrug { get; set; }
        public int RefillQuantity { get; set; }
        public string RefillLot { get; set; }
        public string Drugname { get; set; }
        public DateTime? RefillExpiryDate { get; set; } = DateTime.Today.AddMonths(12);
        public string RefillNotes { get; set; }

        public object objDrug = new object();
        clsStock clsst = new clsStock();
        public PopupRefill(List<RefillRecord> _SelectedDrug)
        {
            InitializeComponent();
            SelectedDrug = _SelectedDrug;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // ปิด popup เมื่อกด OK
            Window.GetWindow(this).Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // ปิด popup เมื่อกด Cancel
            Window.GetWindow(this).Close();
        }

        private void RefillMedicine_Click(object sender, RoutedEventArgs e)
        {
            //ShowPopup();
            // ดึงข้อมูลยาจาก Button's DataContext
            var button = sender as Button;
            var drugData = button?.DataContext;

            if (drugData != null)
            {
                //SelectedDrug = drugData;

                // รีเซ็ตข้อมูลใน popup
                RefillQuantity = 0;
                RefillLot = "";
                RefillExpiryDate = DateTime.Today.AddMonths(12);
                RefillNotes = "";

                // แสดง popup
                RefillPopupOverlay.Visibility = Visibility.Visible;

                // Focus ที่ช่องจำนวน
                //RefillQuantityTextBox.Focus();
            }
        }

        private void CloseRefillPopup_Click(object sender, RoutedEventArgs e)
        {
            // Animation สำหรับซ่อน popup
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            fadeOut.Completed += (s, args) =>
            {
                RefillPopupOverlay.Visibility = Visibility.Collapsed;
            };

            RefillPopupOverlay.BeginAnimation(Grid.OpacityProperty, fadeOut);
        }

        private void CancelRefill_Click(object sender, RoutedEventArgs e)
        {
            //CloseRefillPopup_Click(sender, e);
            Window.GetWindow(this).Close();
        }

        private void ConfirmRefill_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            // ตรวจสอบข้อมูลที่กรอก
            if (ValidateRefillData())
            {
                try
                {                    
                    // บันทึกข้อมูลการเติมยา
                    if(SaveRefillData())
                    {
                        // แสดงข้อความสำเร็จ
                        ShowSuccessMessage();

                        Window.GetWindow(this).Close();

                    }
                    else
                    {

                    }
                   
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"เกิดข้อผิดพลาด: {ex.Message}", "ข้อผิดพลาด",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // รีเฟรชข้อมูล
                    RefreshDrugStocks();
                }
            }
        }

        private bool ValidateRefillData()
        {
            var errors = new List<string>();
            RefillLot = RefillLotTextBox.Text;
            RefillQuantity = Convert.ToInt32(RefillQuantityTextBox.Text);
            RefillExpiryDate = RefillExpiryDatePicker.SelectedDate;
            //DrugCodeText = DrugCodeText.Text;
            Drugname = DrugNameText.Text;

            var refillRecord = new
            {
                DrugCode = DrugCodeText.Text,
                Quantity = RefillQuantity,
                LotNumber = RefillLot,
                ExpiryDate = RefillExpiryDate.Value,
                Notes = RefillNotes,
                RefillDate = DateTime.Now,
                UserId = "CurrentUser" // ใส่ ID ของผู้ใช้ปัจจุบัน
            };

            // ตรวจสอบจำนวน
            if (RefillQuantity <= 0)
            {
                errors.Add("กรุณาระบุจำนวนที่ต้องการเติม");
                //RefillQuantityTextBox.Focus();
            }

            // ตรวจสอบ Lot
            if (string.IsNullOrWhiteSpace(RefillLot))
            {
                errors.Add("กรุณาระบุ Lot Number");
                if (errors.Count == 1) RefillLotTextBox.Focus();
            }

            // ตรวจสอบวันหมดอายุ
            if (!RefillExpiryDate.HasValue)
            {
                errors.Add("กรุณาระบุวันหมดอายุที่ถูกต้อง");
                if (errors.Count == 1) RefillExpiryDatePicker.Focus();
            }

            // แสดงข้อผิดพลาด
            if (errors.Any())
            {
                string errorMessage = string.Join("\n• ", errors.Prepend("กรุณาแก้ไขข้อมูลดังนี้:"));
                MessageBox.Show(errorMessage, "ข้อมูลไม่ครบถ้วน",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool SaveRefillData()
        {
            // TODO: บันทึกข้อมูลลงฐานข้อมูล
            // ตัวอย่างการบันทึก:
            DataTable dt_stock = new DataTable();
            dt_stock = clsst.GetLedStockByCode(DrugCodeText.Text, RefillLotTextBox.Text);
            bool sameLot = false;
            bool result = false;
            int qty_old = 0;
            string shelfzone = "";
            string shelfname = "";
            string max = "";
            string min = "";
            var refillRecord = new
            {
                DrugCode = DrugCodeText.Text,
                Quantity = RefillQuantity,
                LotNumber = RefillLot,
                ExpiryDate = RefillExpiryDate.Value,
                Notes = RefillNotes,
                RefillDate = DateTime.Now,
                UserId = "CurrentUser" // ใส่ ID ของผู้ใช้ปัจจุบัน
            };
            if (dt_stock.Rows.Count > 0)
            {
                foreach (DataRow rw in dt_stock.Rows)
                {
                    if (rw["drugcode"].ToString() == DrugCodeText.Text && rw["lot"].ToString() == RefillLotTextBox.Text)
                    {
                        sameLot = true;
                        qty_old = Convert.ToInt32(rw["Quantity"].ToString());
                        break;
                    }
                    else
                    {
                        shelfzone = rw["location"].ToString();
                        shelfname = rw["drugPosition"].ToString();
                        max = rw["max"].ToString();
                        min = rw["min"].ToString();
                        sameLot = false;
                    }
                }
            }

            if (sameLot) // มี Lot เดิม
            {
                RefillQuantity = qty_old + Convert.ToInt32(RefillQuantityTextBox.Text);
                result = clsst.UpdateStockWhere(refillRecord.DrugCode, RefillQuantity, refillRecord.LotNumber,refillRecord.ExpiryDate.ToString(),refillRecord.UserId);
            }
            else
            {
                result = clsst.InsertStock(refillRecord.DrugCode, RefillQuantity, refillRecord.LotNumber, refillRecord.ExpiryDate.ToString(), shelfzone, shelfname, max, min);
            }


            return result;
            RefreshDrugStocks();
            // อัพเดทสต็อกปัจจุบัน
            // DatabaseService.UpdateDrugStock(drugCode, newQuantity);

            //Console.WriteLine($"บันทึกการเติมยา: {refillRecord}");
        }

        private void ShowSuccessMessage()
        {
            string message = $"เติมยาสำเร็จ!\n\n" +
                            $"ยา: {Drugname}\n" +
                            $"จำนวนที่เติม: {RefillQuantity:N0} หน่วย\n" +
                            $"Lot: {RefillLot}\n" +
                            $"วันหมดอายุ: {RefillExpiryDate:dd/MM/yyyy}";

            MessageBox.Show(message, "เติมยาสำเร็จ",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshDrugStocks()
        {

            DrugCodeText.Text = SelectedDrug[0].DrugCode;
            DrugNameText.Text = SelectedDrug[0].DrugName;
            QuantityText.Text = SelectedDrug[0].Quantity.ToString();
            LocationText.Text = $"{SelectedDrug[0].Location}:  {SelectedDrug[0].DrugPosition}";
            UnitText.Text = "";
            Console.WriteLine("รีเฟรชข้อมูลสต็อกยา");
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDrugStocks();


        }
    }
}
