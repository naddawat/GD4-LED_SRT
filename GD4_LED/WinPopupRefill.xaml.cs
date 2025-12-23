using GD4_LED.cls;
using MySql.Data.MySqlClient;
using Mysqlx;
using Mysqlx.Expr;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
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
using System.Windows.Shapes;

namespace GD4_LED
{
    /// <summary>
    /// Interaction logic for PrescriptionsPopup.xaml
    /// </summary>
    public partial class WinPopupRefill : Window
    {
        public List<RefillRecord> SelectedDrug { get; set; }
        public int RefillQuantity { get; set; }
        public string RefillLot { get; set; }
        public string Drugname { get; set; }
        public DateTime? RefillExpiryDate { get; set; } = DateTime.Today.AddMonths(12);
        public string RefillNotes { get; set; }

        public object objDrug = new object();
        clsQuery _query = new clsQuery();
        public WinPopupRefill(List<RefillRecord> _SelectedDrug)
        {
            InitializeComponent();
            SelectedDrug = _SelectedDrug;
            Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");

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
                    if (SaveRefillData())
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
        public bool SyncDrug(string code)
        {
            DataTable dt_stock = new DataTable();
            dt_stock = _query.GetLedStockByZoneCode(code, clsvariable.shelfzone);
            int position_id = 0;
            int addr = 0;
            string qty = "";
            string LotNo = "";
            string exp = "";
            string orderitemENname = "";
            string position = "";
            string HAD = "";

            foreach (DataRow dr in dt_stock.Rows)
            {
                if (dt_stock.Rows[0]["position_id"].ToString() != "" && dt_stock.Rows[0]["addr"].ToString() != "")
                {
                    position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                    addr = Convert.ToInt32(dt_stock.Rows[0]["addr"].ToString());
                    qty = dt_stock.Rows[0]["In_Qty"].ToString();
                    LotNo = dt_stock.Rows[0]["LotNo"].ToString();
                    exp = dt_stock.Rows[0]["Exp"].ToString();
                    orderitemENname = dt_stock.Rows[0]["orderitemENname"].ToString();
                    position = dt_stock.Rows[0]["shelfname"].ToString();

                    if(dt_stock.Rows[0]["HAD"].ToString() != "")
                    {
                        HAD = dt_stock.Rows[0]["HAD"].ToString();
                    }
                    else
                    {
                        HAD = "0";
                    }

                    //clsvariable.Instance.SerialCan.Order(addr, position_id, qty, orderitemENname, LotNo, exp, position, 0, 0, 0);
                    clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id, orderitemENname, " ",HAD, position);
                    //return true;
                }
                else
                {
                    //return false;
                }
                //clsvariable.Instance.SerialCan.Order(addr, position_id, qty, orderitemENname, LotNo, exp, position, 0, 0, 0);
                //clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id, orderitemENname, "", "", position);
            }

            return true;

        }
        public void LoadStockByCode(string code)
        {
            try
            {
                DataTable dt = new DataTable();
                dt = _query.GetAllLedStockByCode(DrugCodeText.Text, clsvariable.shelfzone);
                string connStr = GD4_LED.Properties.Settings.Default.connectstring;
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

                    //SyncDrug(code);
                }
                else
                {
                    //MessageBox.Show("ไม่มีข้อมูลตำแหน่งยาในตู้ LED นี้", "ข้อมูลว่าง",
                    //              MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.ToString());
            }
            
        }
        public void LoadStock()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = _query.GetAllLedStock(clsvariable.shelfzone);
                string connStr = clsvariable.connectionST;
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
                    MessageBox.Show("ไม่มีข้อมูลตำแหน่งยาในตู้ LED นี้", "ข้อมูลว่าง",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private bool ValidateRefillData()
        {
            var errors = new List<string>();
            RefillLot = RefillLotTextBox.Text;
            if(RefillQuantityTextBox.Text != "")
            {
                RefillQuantity = Convert.ToInt32(RefillQuantityTextBox.Text);
            }
            else
            {
                RefillQuantity = 0;
            }
           
            RefillExpiryDate = RefillExpiryDatePicker.SelectedDate;
            //DrugCodeText = DrugCodeText.Text;
            Drugname = DrugNameText.Text;

            var refillRecord = new
            {
                DrugCode = DrugCodeText.Text,
                Quantity = RefillQuantity,
                LotNumber = RefillLot,
                ExpiryDate = RefillExpiryDate ?? DateTime.Now,
                Notes = RefillNotes,
                RefillDate = DateTime.Now,
                UserId = "CurrentUser"
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
            try
            {
                // TODO: บันทึกข้อมูลลงฐานข้อมูล
                // ตัวอย่างการบันทึก:
                DataTable dt_stock = new DataTable();
                dt_stock = _query.GetLedStockByCode(DrugCodeText.Text, clsvariable.shelfzone);
                bool sameLot = false;
                bool result = false;
                int qty_old = 0;
                string shelfzone = "";
                string shelfname = "";
                string max = "";
                string min = "";
                int addr = 0;
                int position_id = 0;
                string orderitemname = "";
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
                    orderitemname = dt_stock.Rows[0]["drugName"].ToString();
                    foreach (DataRow rw in dt_stock.Rows)
                    {
                        if (rw["drugcode"].ToString() == DrugCodeText.Text && rw["lot"].ToString() == RefillLotTextBox.Text)
                        {
                            addr = Convert.ToInt32(rw["addr"].ToString());
                            position_id = Convert.ToInt32(rw["position_id"].ToString());
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
                else
                {
                    DataTable dt_location = new DataTable();
                    dt_location = _query.GetLocationBycode_ocal(clsvariable.shelfzone, DrugCodeText.Text);
                    if (dt_location.Rows.Count > 0)
                    {
                        orderitemname = dt_location.Rows[0]["orderitemENname"].ToString();
                        addr = Convert.ToInt32(dt_location.Rows[0]["addr"].ToString());
                        position_id = Convert.ToInt32(dt_location.Rows[0]["position_id"].ToString());   
                        shelfzone = dt_location.Rows[0]["shelfzone"].ToString();
                        shelfname = dt_location.Rows[0]["shelfname"].ToString();
                        max = dt_location.Rows[0]["max"].ToString();
                        min = dt_location.Rows[0]["min"].ToString();
                    }
                }

                
                if (sameLot) // มี Lot เดิม
                {
                    RefillQuantity = qty_old + Convert.ToInt32(RefillQuantityTextBox.Text);
                    result = _query.UpdateStockWhere(refillRecord.DrugCode, RefillQuantity, refillRecord.LotNumber, refillRecord.ExpiryDate.ToString(), refillRecord.UserId);
                    
                    clsvariable.Instance.SerialCan.Order(addr, position_id, " ", orderitemname, refillRecord.LotNumber, refillRecord.ExpiryDate.ToString(), RefillQuantity.ToString(), 0, 0, 0);                  
                    Thread.Sleep(2000); // หน่วงเวลา 100 มิลลิวินาที (0.1 วินาที)

                }
                else
                {
                    if (shelfzone != "" && shelfzone != "")
                    {
                        
                        result = _query.InsertStock(refillRecord.DrugCode, RefillQuantity, refillRecord.LotNumber, refillRecord.ExpiryDate.ToString(), shelfzone, shelfname, max, min);
                        //LoadStockByCode(refillRecord.DrugCode);
                        clsvariable.Instance.SerialCan.Order(addr, position_id, " ", orderitemname, refillRecord.LotNumber, refillRecord.ExpiryDate.ToString(), RefillQuantity.ToString(), 0, 0, 0);                       
                        Thread.Sleep(2000); // หน่วงเวลา 100 มิลลิวินาที (0.1 วินาที)
                    }
                    else
                    {
                        MessageBox.Show("ข้อมูลไม่ครบถ้วน");
                    }


                }

                LoadStockByCode(refillRecord.DrugCode);

                //MessageBox.Show(result.ToString());
                return result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

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

        private void RefillQuantityTextBox_TouchUp(object sender, TouchEventArgs e)
        {
            Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
        }

        private void RefillLotTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");

        }
    }
}

