using CrystalDecisions.CrystalReports.Engine;
using GD4_LED.cls;
using GD4_LED.models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
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
using System.Threading;
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
using DrawingBitmap = System.Drawing.Bitmap;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;


namespace GD4_LED
{
    public partial class PrescriptionPopup : Window
    {
        private HashSet<PackageItem> selectedItems = new HashSet<PackageItem>();
        private PrescriptionData prescriptionData;
        private DispatcherTimer timerBtn;
        private DispatcherTimer scanTimer;
        private DispatcherTimer resh;
        bool Cradclick = false;
        clsvariable clsvariable = clsvariable.Instance;
        clsQuery _query = new clsQuery();
        int CountItem = 0;
        private string scannedBarcode = "";
        private bool isVerified = false;

        string jsonString = "";

        public PrescriptionPopup(string _jsonString)
        {
            InitializeComponent();
            //LoadSampleData(jsonString);
            jsonString = _jsonString;
            scanTimer = new DispatcherTimer();
            scanTimer.Interval = TimeSpan.FromMilliseconds(100);
            scanTimer.Tick += ScanTimer_Tick;

            timerBtn = new DispatcherTimer();
            timerBtn.Interval = TimeSpan.FromSeconds(1);
            timerBtn.Tick += Timer_Tick;
            
            //this.Loaded += (s, e) => this.Foccus();
            //MainScrollViewer => this.Focus();
            this.KeyDown += Window_KeyDown;
            //timer.Start();
            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            if (isVerified) return;

            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(scannedBarcode))
                {
                    VerifyDispenser(scannedBarcode,"");
                    scannedBarcode = "";


                    LoadSampleData(jsonString);

                }
            }
            else if (e.Key >= Key.D0 && e.Key <= Key.Z || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {

                string key = e.Key.ToString().Replace("D", "").Replace("NumPad", "");
                scannedBarcode += key;

                txtScannedCode.Text = scannedBarcode;
                txtScannedCode.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#3b82f6"));


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
                VerifyDispenser(scannedBarcode,"");
                scannedBarcode = "";


                LoadSampleData(jsonString);

            }
        }

        private void VerifyDispenser(string code,string password)
        {            
            if (string.IsNullOrEmpty(code))
            {                
                txtVerificationStatus.Text = "⚠️ รหัสไม่ถูกต้อง กรุณาสแกนใหม่อีกครั้ง";
                txtVerificationStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#dc2626"));
                txtVerificationStatus.Visibility = Visibility.Visible;

                txtScannedCode.Text = "รอการสแกน...";
                txtScannedCode.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#3b82f6"));
                return;
            }
            else
            {
                DataTable dt_user = new DataTable();
                dt_user = _query.GetUser(code, password);
                if(dt_user.Rows.Count > 0)
                {

                    txtname.Text = dt_user.Rows[0]["fullname"].ToString();
                    isVerified = true;

                    txtStatus.Text = dt_user.Rows[0]["fullname"].ToString();
                    txtScannedCode.Text = code;
                    txtScannedCode.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#10b981"));
                    txtVerificationStatus.Text = "✓ ยืนยันตัวตนสำเร็จ";
                    txtVerificationStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#10b981"));
                    txtVerificationStatus.Visibility = Visibility.Visible;

                    timerBtn.Start();

                }
                else
                {
                    txtVerificationStatus.Text = "⚠️ ไม่พบข้อมูลผู้ใช้ กรุณาสแกนใหม่อีกครั้ง";
                    txtVerificationStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#dc2626"));
                    txtVerificationStatus.Visibility = Visibility.Visible;
                    txtScannedCode.Text = "รอการสแกน...";
                    txtScannedCode.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#3b82f6"));
                    return;
                }
            }

          
  
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
                //timer.Start();
            };

            verificationOverlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void PrintPrescription(PrescriptionData prescription)
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
        private void PrintPrescriptionNoLed(object prescription)
        {
            timerBtn.Stop();
            string orderitemcode = ((dynamic)prescription).OrderItemCode;
            int qty = ((dynamic)prescription).OrderQty;
            string addr = ((dynamic)prescription).Addr;
            string id = ((dynamic)prescription).id;

            DataTable dt_stock = new DataTable();
            dt_stock = _query.GetLedStockByAddr(addr, id, clsvariable.shelfzone);
            if (dt_stock.Rows.Count != 0)
            {
                InvenStock(dt_stock.Rows[0]["drugCode"].ToString(), qty.ToString());
            }
            else
            {
                clsvariable.StrU = "";
            }

            //for (int i = 0; i < prescription.Package.Count; i++)
            //{
            //    orderitemcode = prescription.Package[i].OrderItemCode.ToString();
            //    qty = prescription.Package[i].OrderQty.ToString();

            //    if (orderitemcode != "")
            //    {
            //        //ShowLed(orderitemcode, qty);
            //    }
            //}

            //clsvariable.dt_Prescr = _query.GetPrescriptionByCode(prescription.PrescriptionNo.ToString());
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

                    clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id, orderitemENname, "", "", position);

                    //return true;
                }
                else
                {
                    //return false;
                }
                clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id, orderitemENname, "", "", position);
            }

            return true;

        }

        public void ShowLed(string orderitem, string qty)
        {
            DataTable dt_stock = new DataTable();
            bool result = false;
            dt_stock = _query.GetStockByCode(orderitem);
            int order_qty = Convert.ToInt32(qty);
            if (dt_stock.Rows.Count > 0)
            {
                int position_id =0;
                int addr =0;
                int R = Convert.ToInt32(clsvariable.RGD_dispense[0].ToString());
                int G = Convert.ToInt32(clsvariable.RGD_dispense[1].ToString());
                int B = Convert.ToInt32(clsvariable.RGD_dispense[2].ToString());
                if(dt_stock.Rows[0]["position_id"].ToString() != "")
                {
                    position_id = Convert.ToInt32(dt_stock.Rows[0]["position_id"].ToString());
                }
                
                if(dt_stock.Rows[0]["addr"].ToString() != "")
                {
                    addr = Convert.ToInt32(dt_stock.Rows[0]["addr"].ToString());
                }
                
                string LotNo = dt_stock.Rows[0]["LotNo"].ToString();
                string Exp = Convert.ToDateTime(dt_stock.Rows[0]["Exp"]).ToString("dd-MM-yyyy", CultureInfo.GetCultureInfo("en-US"));
                string shelfname = dt_stock.Rows[0]["shelfname"].ToString();
                string In_Qty = dt_stock.Rows[0]["In_Qty"].ToString();
                clsvariable.Instance.SerialCan.Order(addr, position_id, qty, shelfname, LotNo, Exp, In_Qty, R, G, B);
            }
            else
            {
                MessageBox.Show($"ไม่พบข้อมูลสต๊อกของ {orderitem} กรุณาตรวจสอบ stock ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            timerBtn?.Stop();
            scanTimer?.Stop();
            //var page = new GD4_LED.page.DispensePage();
            //await page.InitialqizePageAsync();

            //((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(page);
            this.Close();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(clsvariable.StrU_array[2]))
            {
                timerBtn.Stop();
                if (Convert.ToInt32(clsvariable.StrU_array[2]) > 0)
                {
                    DataTable dt_stock = new DataTable();
                    dt_stock = _query.GetLedStockByAddr(clsvariable.StrU_array[0], clsvariable.StrU_array[1], clsvariable.shelfzone);
                    if (dt_stock.Rows.Count != 0)
                    {
                        InvenStock(dt_stock.Rows[0]["drugCode"].ToString(), clsvariable.StrU_array[2]);

                        clsvariable.StrU = "";
                        clsvariable.StrU_array = new string[3];

                        
                    }
                    else
                    {
                        clsvariable.StrU = "";
                    }
                }
            }
            if(txtTotalItems.Text == txtSelectedItems.Text)
            {
                timerBtn.Start();
                CountItem = 0;
                
            }
            else
            {
                timerBtn.Start();
                //CountItem =+1;
            }

            clsvariable.StrU = "";
            clsvariable.StrU_array = new string[3];
        }

        public async void InvenStock(string orderitem, string qty)
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
            db_print.Columns.Add("ward", typeof(String));
            db_print.Columns.Add("orderitemnameTH", typeof(String));
            db_print.Columns.Add("QRcode", typeof(byte[]));
            db_print.Columns.Add("location", typeof(String));
            db_print.TableName = "db_print";
           
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
                        timerBtn.Stop();

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
                                remaining_qty = 0; // ตัดครบแล้ว
                            }
                            else // ไม่พอ ต้องใช้ lot ถัดไป
                            {
                                result = _query.UpdateDisStock("0", orderitem, lot, exp);
                                if (result) Debug.WriteLine($"Update stock {orderitem} new qty: 0");
                                remaining_qty = Math.Abs(new_qty); // ยังเหลือให้ตัดแถวถัดไป
                            }

                            index++;
                        }

                        if (result)
                        {
                            result = _query.UpdateJob(txtStatus.Text, prescriptionno, seq, orderitem);
                            //SyncDrug(orderitem);
                            LoadStockByCode(orderitem);
                        }
                        else
                        {
                            Debug.WriteLine($"Update stock {orderitem} error");
                        }

                        foreach (DataRow row in dt_prescr.Rows)
                        {
                            DataRow r = db_print.Rows.Add();
                            r["prescriptionno"] = row["prescriptionno"].ToString();
                            r["orderitemname"] = row["orderitemname"].ToString();
                            r["orderitemnameTH"] = row["orderitemnameTH"].ToString();
                            r["orderqty"] = row["orderqty"].ToString().Split('.')[0];
                            r["patientname"] = row["patientname"].ToString();
                            r["hn"] = row["hn"].ToString();
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
                                LoadReport(db_print);
                            }
                        }
                        //timer.Start();

                        CountItem += 1;
                        txtSelectedItems.Text = CountItem.ToString();
                        clsvariable.StrU = "";
                        clsvariable.StrU_array = new string[3];
                    }

                    if (CountItem == prescriptionData.Package.Count)
                    {
                        timerBtn.Stop();
                        txtSelectedItems.Text = CountItem.ToString();
                        clsvariable.StrU = "";
                        clsvariable.StrU_array = new string[3];
                        CountItem = 0;
                        //Thread.Sleep(3000); // หน่วงเวลา 100 มิลลิวินาที (0.1 วินาที)

                        var page = new GD4_LED.page.DispensePage();
                        await page.InitializePageAsync();
                        ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(page);

                        this.Close();
                 
                    }

                }
            }
        }
        public static System.Drawing.Image genQr(string txt)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            var QRCodeData = qrGenerator.CreateQrCode(txt, QRCodeGenerator.ECCLevel.M);
            QRCode QRCode = new QRCode(QRCodeData);
            DrawingBitmap qrCodeImage = QRCode.GetGraphic(20);

            return qrCodeImage;
        }
        public void LoadStockByCode(string code)
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

                SyncDrug(code);
            }
            else
            {
                MessageBox.Show("ไม่มีข้อมูลตำแหน่งยาในตู้ LED นี้", "ข้อมูลว่าง",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadReport(DataTable dt)
        {
            try
            {

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            //MessageBox.Show(clsvariable.crp_report + " " + clsvariable.printname);
            ReportDocument report = new ReportDocument();
            report.Load(clsvariable.crp_report);
            report.SetDataSource(dt);
            report.PrintOptions.PrinterName = clsvariable.printname;
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
            
            prescriptionData = JsonConvert.DeserializeObject<PrescriptionData>(jsonData);

            txtPrescriptionNo.Text = $"เลขที่ใบสั่ง: {prescriptionData.PrescriptionNo}";
            txtPatientName.Text = prescriptionData.PatientName;
            txtHN.Text = prescriptionData.HN;
            txtAN.Text = prescriptionData.AN;
            txtWard.Text = prescriptionData.Ward;
            txtBed.Text = prescriptionData.Bed;
            //txtStatus.Text = prescriptionData.Status;

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
                    statusBorder.Background = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#fef3c7"));
                    txtStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#f59e0b"));
                    break;
                case "จัดแล้ว":
                    statusBorder.Background = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#dcfce7"));
                    txtStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#16a34a"));
                    break;
                case "ยกเลิก":
                    statusBorder.Background = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#fee2e2"));
                    txtStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#dc2626"));
                    break;
                default:
                    statusBorder.Background = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#f1f5f9"));
                    txtStatus.Foreground = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#64748b"));
                    break;
            }
        }

        public void PackageCard_Click(object sender, MouseButtonEventArgs e)
        {
            
            if (PackageItemsControl != null) 
            {
                Cradclick = true;
                var border = sender as System.Windows.Controls.Border;
                if (border == null) return;

                var item = border.Tag as PackageItem;
                if (item == null) return;
                //selectedItems.Add(item);

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

                PrintPrescriptionNoLed(item);
            }

            Cradclick= false;


        }

        private void AnimateCheckMark(Border checkMark, Border cardBorder, bool show)
        {
            if (show)
            {
                checkMark.Visibility = Visibility.Visible;

                var colorAnimation = new ColorAnimation
                {
                    To = (MediaColor)MediaColorConverter.ConvertFromString("#10b981"),
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

                cardBorder.BorderBrush = new SolidColorBrush((MediaColor)MediaColorConverter.ConvertFromString("#10b981"));
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

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการยาอย่างน้อย 1 รายการ", "แจ้งเตือน",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //string selectedList = string.Join("\n", selectedItems.Select(x => $"- {x.OrderItemName} (จำนวน: {x.OrderQty})"));
            //var result = MessageBox.Show($"ยืนยันการเลือกรายการยา {selectedItems.Count} รายการ?\n\n{selectedList}",
            //    "ยืนยัน", MessageBoxButton.YesNo, MessageBoxImage.Question);

            //if (result == MessageBoxResult.Yes)
            //{
            //    MessageBox.Show("บันทึกข้อมูลสำเร็จ!", "สำเร็จ", MessageBoxButton.OK, MessageBoxImage.Information);
            //    this.Close();
            //}
            //SetActiveTab(DispenseButton);
            //MainFrame.Navigate(new DispensePage());

            var page = new GD4_LED.page.DispensePage();
            await page.InitializePageAsync();
            ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(page);

            this.Close();


        }

        public List<PackageItem> GetSelectedItems()
        {
            return selectedItems.ToList();
        }

        private void txtScannedCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(txtScannedCode.Text))
                {
                    VerifyDispenser(txtScannedCode.Text,"");
                    scannedBarcode = "";

                    LoadSampleData(jsonString);
                    
                }
            }
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