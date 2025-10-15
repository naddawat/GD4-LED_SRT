using CrystalDecisions.CrystalReports.Engine;
using GD4_LED.cls;
using GD4_LED.models;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Border = System.Windows.Controls.Border;

namespace GD4_LED
{
    public partial class PrescriptionPopup : Window
    {
        private HashSet<PackageItem> selectedItems = new HashSet<PackageItem>();
        private Prescription prescriptionData;
        private DispatcherTimer timer;
        private DispatcherTimer scanTimer;
        clsvariable clsvariable = clsvariable.Instance;
        clsQuery _query = new clsQuery();
        int CountItem = 0;
        private string scannedBarcode = "";
        private bool isVerified = false;

        public PrescriptionPopup(string jsonString)
        {
            InitializeComponent();
            LoadSampleData(jsonString);

            scanTimer = new DispatcherTimer();
            scanTimer.Interval = TimeSpan.FromMilliseconds(100);
            scanTimer.Tick += ScanTimer_Tick;

            this.Loaded += (s, e) => this.Focus();
            this.KeyDown += Window_KeyDown;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (isVerified) return;

            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(scannedBarcode))
                {
                    VerifyDispenser(scannedBarcode);
                    scannedBarcode = "";
                }
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.Z || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {

                string key = e.Key.ToString().Replace("D", "").Replace("NumPad", "");
                scannedBarcode += key;

                txtScannedCode.Text = scannedBarcode;
                txtScannedCode.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b82f6"));


                scanTimer.Stop();
                scanTimer.Start();
            }
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            // Auto-verify after brief pause in scanning
            scanTimer.Stop();
            if (!string.IsNullOrEmpty(scannedBarcode))
            {
                VerifyDispenser(scannedBarcode);
                scannedBarcode = "";
            }
        }

        private void VerifyDispenser(string code)
        {

            if (string.IsNullOrEmpty(code) || code.Length < 4)
            {
                txtVerificationStatus.Text = "⚠️ รหัสไม่ถูกต้อง กรุณาสแกนใหม่อีกครั้ง";
                txtVerificationStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc2626"));
                txtVerificationStatus.Visibility = Visibility.Visible;

                txtScannedCode.Text = "รอการสแกน...";
                txtScannedCode.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b82f6"));
                return;
            }

            isVerified = true;


            txtStatus.Text = code;

            txtScannedCode.Text = code;
            txtScannedCode.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
            txtVerificationStatus.Text = "✓ ยืนยันตัวตนสำเร็จ";
            txtVerificationStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
            txtVerificationStatus.Visibility = Visibility.Visible;

  
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(500),
                BeginTime = TimeSpan.FromMilliseconds(500)
            };

            fadeOut.Completed += (s, e) =>
            {
                verificationOverlay.Visibility = Visibility.Collapsed;
                // Start the main timer after verification
                timer.Start();
            };

            verificationOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void PrintPrescription(Prescription prescription)
        {
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

        public void ShowLed(string orderitem, string qty)
        {
            DataTable dt_stock = new DataTable();
            bool result = false;
            dt_stock = _query.GetStockByCode(orderitem);
            int order_qty = Convert.ToInt32(qty);
            if (dt_stock.Rows.Count > 0)
            {
                int R = Convert.ToInt32(clsvariable.RGD_dispense[0].ToString());
                int G = Convert.ToInt32(clsvariable.RGD_dispense[1].ToString());
                int B = Convert.ToInt32(clsvariable.RGD_dispense[2].ToString());
                int position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                clsvariable.Instance.SerialCan.Order(1, position_id, qty, "", "", "", "", R, G, B);
            }
            else
            {
                MessageBox.Show($"ไม่พบข้อมูลสต๊อกของ {orderitem} กรุณาตรวจสอบ stock ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            timer?.Stop();
            scanTimer?.Stop();
            this.Close();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(clsvariable.StrU_array[2]))
            {
                if (Convert.ToInt32(clsvariable.StrU_array[2]) > 0)
                {
                    DataTable dt_stock = new DataTable();
                    dt_stock = _query.GetLedStockByAddr(clsvariable.StrU_array[0], clsvariable.StrU_array[1], clsvariable.shelfzone);
                    if (dt_stock.Rows.Count != 0)
                    {
                        InvenStock(dt_stock.Rows[0]["drugCode"].ToString(), clsvariable.StrU_array[2]);
                    }
                    else
                    {
                        clsvariable.StrU = "";
                    }
                }
            }

            clsvariable.StrU = "";
            clsvariable.StrU_array = new string[3];
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
                if (dt_stock.Rows.Count == 1)
                {
                    foreach (DataRow row in dt_stock.Rows)
                    {
                        string prescriptionno = clsvariable.dt_Prescr.Rows[0]["prescriptionno"].ToString();
                        string seq = clsvariable.dt_Prescr.Rows[0]["seq"].ToString();
                        string orderitemcode = clsvariable.dt_Prescr.Rows[0]["orderitemcode"].ToString();
                        int stock_qty = Convert.ToInt32(row["In_Qty"].ToString());
                        int new_qty = stock_qty - order_qty;
                        string lot = row["LotNo"].ToString();
                        string exp = row["Exp"].ToString();

                        result = _query.UpdateDisStock(new_qty.ToString(), orderitem, lot, exp);
                        if (result)
                        {
                            Debug.WriteLine($"Update stock {orderitem} new qty: {new_qty}");
                            result = _query.UpdateJob("USER", prescriptionno, seq, orderitemcode);
                        }
                        else
                        {
                            Debug.WriteLine($"Update stock {orderitem} error");
                        }
                    }
                }
                else if (dt_stock.Rows.Count > 1)
                {
                    foreach (DataRow row in dt_stock.Rows)
                    {
                        string prescriptionno = clsvariable.dt_Prescr.Rows[0]["prescriptionno"].ToString();
                        string seq = clsvariable.dt_Prescr.Rows[0]["seq"].ToString();
                        string orderitemcode = clsvariable.dt_Prescr.Rows[0]["orderitemcode"].ToString();
                        int stock_qty = Convert.ToInt32(row["In_Qty"].ToString());
                        int new_qty = stock_qty - order_qty;
                        string lot = row["LotNo"].ToString();
                        string exp = row["Exp"].ToString();
                        result = _query.UpdateDisStock(new_qty.ToString(), orderitem, lot, exp);
                        if (result)
                        {
                            Debug.WriteLine($"Update stock {orderitem} new qty: {new_qty}");
                            result = _query.UpdateJob("USER", prescriptionno, seq, orderitemcode);
                        }
                        else
                        {
                            Debug.WriteLine($"Update stock {orderitem} error");
                        }

                        if (new_qty == 0)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"ไม่พบข้อมูลสต๊อกของ {orderitem} กรุณาตรวจสอบ stock ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

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

                if (clsvariable.print_isenable)
                {
                    if (db_print.Rows.Count > 0)
                    {
                        LoadReport(db_print);
                    }
                }

                CountItem += 1;
                txtSelectedItems.Text = CountItem.ToString();
                clsvariable.StrU = "";
                clsvariable.StrU_array = new string[3];
            }

            if (CountItem >= prescriptionData.Package.Count)
            {
                timer.Stop();
                this.Close();
            }
        }

        private void LoadReport(DataTable dt)
        {
            ReportDocument report = new ReportDocument();
            report.Load(@"D:\GitHub\GD4-LED_SRT\GD4_LED\report\crp_stricker.rpt");
            report.SetDataSource(dt);
            report.PrintOptions.PrinterName = "";
            report.PrintToPrinter(1, false, 0, 0);
            report.Close();
            report.Dispose();
        }

        private void LoadSampleData(string jsonString)
        {
            string jsonData = jsonString;
            LoadData(jsonData);
        }

        public void LoadData(string jsonData)
        {
            prescriptionData = JsonConvert.DeserializeObject<Prescription>(jsonData);

            txtPrescriptionNo.Text = $"เลขที่ใบสั่ง: {prescriptionData.PrescriptionNo}";
            txtPatientName.Text = prescriptionData.PatientName;
            txtHN.Text = prescriptionData.HN;
            txtAN.Text = prescriptionData.AN;
            txtWard.Text = prescriptionData.Ward;
            txtBed.Text = prescriptionData.Bed;
            txtStatus.Text = prescriptionData.Status;

            SetStatusColor(prescriptionData.Status);
            PackageItemsControl.ItemsSource = prescriptionData.Package;
            UpdateFooter();

            // Don't print until verified
            if (isVerified)
            {
                PrintPrescription(prescriptionData);
            }
        }

        private void SetStatusColor(string status)
        {
            switch (status)
            {
                case "รอจัด":
                    statusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fef3c7"));
                    txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f59e0b"));
                    break;
                case "จัดแล้ว":
                    statusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dcfce7"));
                    txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16a34a"));
                    break;
                case "ยกเลิก":
                    statusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee2e2"));
                    txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dc2626"));
                    break;
                default:
                    statusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9"));
                    txtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748b"));
                    break;
            }
        }

        public void PackageCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var item = border.Tag as PackageItem;
            if (item == null) return;

            var grid = border.Child as Grid;
            if (grid == null) return;

            var checkMark = FindVisualChild<Border>(grid, "checkMark");
            if (checkMark == null) return;

            if (selectedItems.Contains(item))
            {
                selectedItems.Remove(item);
                AnimateCheckMark(checkMark, border, false);
            }
            else
            {
                selectedItems.Add(item);
                AnimateCheckMark(checkMark, border, true);
            }

            UpdateFooter();
        }

        private void AnimateCheckMark(Border checkMark, Border cardBorder, bool show)
        {
            if (show)
            {
                checkMark.Visibility = Visibility.Visible;

                var colorAnimation = new ColorAnimation
                {
                    To = (Color)ColorConverter.ConvertFromString("#10b981"),
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                checkMark.Background = new SolidColorBrush(Colors.Transparent);
                checkMark.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                var scaleTransform = new ScaleTransform(0, 0, 30, 30);
                checkMark.RenderTransform = scaleTransform;

                var scaleXAnimation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new BackEase { Amplitude = 0.3 }
                };
                var scaleYAnimation = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new BackEase { Amplitude = 0.3 }
                };

                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);

                cardBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
                cardBorder.BorderThickness = new Thickness(2);
            }
            else
            {
                var opacityAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                opacityAnimation.Completed += (s, e) => checkMark.Visibility = Visibility.Collapsed;
                checkMark.BeginAnimation(Border.OpacityProperty, opacityAnimation);

                cardBorder.BorderThickness = new Thickness(0);
            }
        }

        private void UpdateFooter()
        {
            int totalItems = prescriptionData?.Package?.Count ?? 0;
            int selectedCount = selectedItems.Count;

            txtTotalItems.Text = totalItems.ToString();
            txtSelectedItems.Text = selectedCount.ToString();
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T && (child as FrameworkElement)?.Name == childName)
                {
                    return (T)child;
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการยาอย่างน้อย 1 รายการ", "แจ้งเตือน",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedList = string.Join("\n", selectedItems.Select(x => $"- {x.OrderItemName} (จำนวน: {x.OrderQty})"));
            var result = MessageBox.Show($"ยืนยันการเลือกรายการยา {selectedItems.Count} รายการ?\n\n{selectedList}",
                "ยืนยัน", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("บันทึกข้อมูลสำเร็จ!", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }

        public List<PackageItem> GetSelectedItems()
        {
            return selectedItems.ToList();
        }
    }

    public class PrescriptionData
    {
        public string PrescriptionNo { get; set; }
        public string HN { get; set; }
        public string AN { get; set; }
        public string PatientName { get; set; }
        public string Ward { get; set; }
        public string Bed { get; set; }
        public string Status { get; set; }
        public List<PackageItem> Package { get; set; }
    }

    public class PackageItem
    {
        public string OrderItemCode { get; set; }
        public string OrderItemName { get; set; }
        public int OrderQty { get; set; }

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