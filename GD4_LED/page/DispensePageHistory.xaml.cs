using CrystalDecisions.CrystalReports.Engine;
using GD4_LED.cls;
using GD4_LED.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
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
using Border = System.Windows.Controls.Border;

namespace GD4_LED.page
{
    /// <summary>
    /// Interaction logic for DispensePageHistory.xaml
    /// </summary>
    public partial class DispensePageHistory : Page
    {
        //private readonly RxService _RX;
        private List<Prescription> allPrescriptions = new List<Prescription>();
        private List<Prescription> filteredPrescriptions = new List<Prescription>();
        private bool _isLoading = true;
        clsQuery _query = new clsQuery();
        public DispensePageHistory()
        {
            InitializeComponent();
            // เริ่ม Loading Animation
            StartLoadingAnimation();

            // โหลดข้อมูลแบบ Async
            _ = InitializePageAsync();
            //_RX = new RxService();
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

        private async Task InitializePageAsync()
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

        private void LoadPrescriptions()
        {
            DataTable dtPrescriptions = new DataTable();
            dtPrescriptions = _query.GetPrescrfinished(clsvariable.shelfzone);
            List<Prescription> list = new List<Prescription>();

            foreach (DataRow row in dtPrescriptions.Rows)
            {
                var prescriptions = dtPrescriptions.AsEnumerable()
                .GroupBy(r => new
                {
                    prescriptionno = r["prescriptionno"].ToString(),
                    hn = r["hn"].ToString(),
                    an = r["an"].ToString(),
                    patientname = r["patientname"].ToString(),
                    ward = r["ward"].ToString(),
                    bed = r["bed"].ToString()
                })
                .Select(g => new
                {
                    prescriptionno = g.Key.prescriptionno,
                    hn = g.Key.hn,
                    an = g.Key.an,
                    patientname = g.Key.patientname,
                    ward = g.Key.ward,
                    bed = g.Key.bed,
                    status = "เสร็จแล้ว", // ใส่เอง เพราะไม่มีใน SQL
                    package = g.Select(r => new
                    {
                        orderitemcode = r["orderitemcode"].ToString(),
                        orderitemname = r["orderitemname"].ToString(),
                        orderqty = Convert.ToInt32(r["orderqty"])
                    }).ToList()
                })
                .ToList();

                // แปลง JSON
                string jsonResult = JsonConvert.SerializeObject(prescriptions, Formatting.Indented);

                allPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(jsonResult);
                filteredPrescriptions = allPrescriptions.ToList();
                DisplayPrescriptions();
            }
        }

        private void UpdateStatistics()
        {
            int pendingCount = filteredPrescriptions.Count(p => p.Status == "เสร็จแล้ว");
            int completedCount = filteredPrescriptions.Count(p => p.Status == "เสร็จแล้ว");
            int totalCount = filteredPrescriptions.Count;

            //PendingCountText.Text = pendingCount.ToString();
            CompletedCountText.Text = pendingCount.ToString();
            //TotalCountText.Text = totalCount.ToString();
        }

        private void DisplayPrescriptions()
        {
            PrescriptionPanel.Children.Clear();

            foreach (var prescription in filteredPrescriptions)
            {
                CreatePrescriptionCard(prescription);
            }
        }
        private void CreatePrescriptionCard(Prescription prescription)
        {
            Border cardBorder = new Border();
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
            Border statusBorder = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4)
            };

            // Set color based on status
            if (prescription.Status == "รอจัด")
            {
                statusBorder.Background = (Brush)FindResource("Warning");
            }
            else if (prescription.Status == "เสร็จแล้ว")
            {
                statusBorder.Background = (Brush)FindResource("Success");
            }

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
                FontSize = 12,
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
                Content = "ปริ้น",
                Width = 80,
                Height = 36,
                Margin = new Thickness(0, 0, 8, 0),
                FontSize = 16,
                Style = (Style)FindResource("PrintButtonStyle")
                //Content = "ปริ้น",
                //Width = 80,
                //Height = 36,
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
                Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Visibility = Visibility.Collapsed

            };

            // Add button click events
            printButton.Click += (sender, e) => {
                e.Handled = true; // Prevent card toggle
                PrintPrescription(prescription);
            };

            cancelButton.Click += (sender, e) => {
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
                Border packageBorder = new Border
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
            packageSummary.MouseLeftButtonUp += (sender, e) => {
                e.Handled = true;
                ToggleCardDetails(medicinePanel, expandIcon);
            };
            packageSummary.TouchUp += (sender, e) => {
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
            PerformSearch();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }


        private void PerformSearch()
        {
            string searchText = SearchTextBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredPrescriptions = allPrescriptions.ToList();
            }
            else
            {
                filteredPrescriptions = allPrescriptions.Where(p =>
                    p.PatientName.ToLower().Contains(searchText) ||
                    p.HN.ToLower().Contains(searchText) ||
                    p.PrescriptionNo.ToLower().Contains(searchText) ||
                    p.Ward.ToLower().Contains(searchText)
                ).ToList();
            }

            DisplayPrescriptions();
            UpdateStatistics();
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (_RX != null)
            //{
            //    _RX.OnTriggerReceived -= Rx_OnTriggerReceived;
            //}

            // หยุด Animations
            var loadingStoryboard = (Storyboard)this.Resources["LoadingAnimation"];
            loadingStoryboard?.Stop();
        }

        private void PrintPrescription(Prescription prescription)
        {
            MessageBox.Show($"พิมพ์ใบสั่งยาหมายเลข: {prescription.PrescriptionNo}\nผู้ป่วย: {prescription.PatientName}",
                          "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Information);

            clsvariable.dt_Prescr_his = _query.GetPrescriptionByCodeHis(prescription.PrescriptionNo);
            if (clsvariable.dt_Prescr_his.Rows.Count > 0)
            {
                foreach(DataRow row in clsvariable.dt_Prescr_his.Rows)
                {
                    InvenStock(row["orderitemcode"].ToString());
                }
            }
        }
        public void InvenStock(string orderitem)
        {
            DataTable dt_stock = new DataTable();
            DataTable dt_regist_flag = new DataTable();
            bool result = false;
            string regist_flag = "";           
            dt_stock = _query.GetStockByCode(orderitem);
            //int order_qty = Convert.ToInt32(qty);

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

            if (clsvariable.dt_Prescr_his.Rows.Count > 0)
            {
                DataTable dt_prescr = new DataTable();
                DataRow[] rows = clsvariable.dt_Prescr_his.Select("orderitemcode = '" + orderitem.Replace("'", "''") + "'");

                if (rows.Length > 0)
                {
                    dt_prescr = rows.CopyToDataTable();
                }
                else
                {
                    dt_prescr = clsvariable.dt_Prescr_his.Clone(); // คืน DataTable โครงสร้างเดิมแต่ไม่มีข้อมูล
                }

                if (dt_prescr.Rows.Count > 0)
                {
                    string prescriptionno = dt_prescr.Rows[0]["prescriptionno"].ToString();
                    string seq = dt_prescr.Rows[0]["seq"].ToString();
                    dt_regist_flag = _query.GetRegist_flag(dt_prescr.Rows[0]["hn"].ToString());
                    if (dt_regist_flag.Rows.Count > 0)
                    {
                        regist_flag = dt_regist_flag.Rows[0]["regist_flag"].ToString();
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
                        PrescriptionPopup.genQr(row["prescriptionno"].ToString()).Save(mss, System.Drawing.Imaging.ImageFormat.Jpeg);
                        bytess = mss.ToArray();
                        if (bytess.Length > 0)
                        {
                            r["QRcode"] = bytess;
                        }
                        else
                        {
                            r["QRcode"] = "";
                        }
                        r["location"] = row["shelfname"].ToString();
                    }

                    if (db_print.Rows.Count > 0)
                    {
                        LoadReport(db_print);
                    }
                }
            }
        }
        private void LoadReport(DataTable dt)
        {
            try
            {

            }
            catch (Exception ex)
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



    }
}
