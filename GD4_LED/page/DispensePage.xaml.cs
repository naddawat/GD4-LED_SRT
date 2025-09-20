
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using GD4_LED.models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GD4_LED.page
{
    public partial class DispensePage : Page
    {
        private readonly RxService _RX;
        private List<Prescription> allPrescriptions = new List<Prescription>();
        private List<Prescription> filteredPrescriptions = new List<Prescription>();
        public DispensePage()
        {
            InitializeComponent();
            SetupSmoothScrolling();
            LoadPrescriptions();
            UpdateStatistics();
            _RX = new RxService();
            //_RX.OnTriggerReceived += Rx_OnTriggerReceived;


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
            string jsonData = @"[
    {
        ""prescriptionno"": ""1892-2"",
        ""hn"": ""1892"",
        ""an"": ""2"",
        ""patientname"": ""นาย TEST1 TEST2"",
        ""ward"": ""Ward A"",
        ""bed"": ""12"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P001"",
                ""orderitemname"": ""Package 1"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P002"",
                ""orderitemcode"": ""Package 2"",
                ""orderqty"": 2
            }
        ]
    },
    {
        ""prescriptionno"": ""1893-1"",
        ""hn"": ""1893"",
        ""an"": ""1"",
        ""patientname"": ""นาง TEST3 TEST4"",
        ""ward"": ""Ward B"",
        ""bed"": ""5"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P003"",
                ""orderitemname"": ""Package 3"",
                ""orderqty"": 1
            }
        ]
    },
    {
        ""prescriptionno"": ""1894-3"",
        ""hn"": ""1894"",
        ""an"": ""3"",
        ""patientname"": ""นาย TEST5 TEST6"",
        ""ward"": ""Ward C"",
        ""bed"": ""8"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P004"",
                ""orderitemname"": ""Package 4"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P005"",
                ""orderitemname"": ""Package 5"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P006"",
                ""orderitemname"": ""Package 6"",
                ""orderqty"": 3
            }
        ]
    },
    {
        ""prescriptionno"": ""1895-4"",
        ""hn"": ""1895"",
        ""an"": ""4"",
        ""patientname"": ""นางสาว TEST7 TEST8"",
        ""ward"": ""Ward D"",
        ""bed"": ""15"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P007"",
                ""orderitemname"": ""Package 7"",
                ""orderqty"": 2
            },
            {
                ""orderitemcode"": ""P008"",
                ""orderitemname"": ""Package 8"",
                ""orderqty"": 1
            }
        ]
    },
    {
        ""prescriptionno"": ""1896-5"",
        ""hn"": ""1896"",
        ""an"": ""5"",
        ""patientname"": ""นาย TEST9 TEST10"",
        ""ward"": ""Ward E"",
        ""bed"": ""22"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P009"",
                ""orderitemname"": ""Package 9"",
                ""orderqty"": 1
            }
        ]
    },
    {
        ""prescriptionno"": ""1897-6"",
        ""hn"": ""1897"",
        ""an"": ""6"",
        ""patientname"": ""นาง TEST11 TEST12"",
        ""ward"": ""Ward F"",
        ""bed"": ""7"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P010"",
                ""orderitemname"": ""Package 10"",
                ""orderqty"": 3
            },
            {
                ""orderitemcode"": ""P011"",
                ""orderitemname"": ""Package 11"",
                ""orderqty"": 2
            }
        ]
    },
    {
        ""prescriptionno"": ""1898-7"",
        ""hn"": ""1898"",
        ""an"": ""7"",
        ""patientname"": ""นาย TEST13 TEST14"",
        ""ward"": ""Ward G"",
        ""bed"": ""18"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P012"",
                ""orderitemname"": ""Package 12"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P013"",
                ""orderitemname"": ""Package 13"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P014"",
                ""orderitemname"": ""Package 14"",
                ""orderqty"": 1
            }
        ]
    },
    {
        ""prescriptionno"": ""1899-8"",
        ""hn"": ""1899"",
        ""an"": ""8"",
        ""patientname"": ""นางสาว TEST15 TEST16"",
        ""ward"": ""Ward H"",
        ""bed"": ""9"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P015"",
                ""orderitemname"": ""Package 15"",
                ""orderqty"": 2
            }
        ]
    },
    {
        ""prescriptionno"": ""1900-9"",
        ""hn"": ""1900"",
        ""an"": ""9"",
        ""patientname"": ""นาย TEST17 TEST18"",
        ""ward"": ""Ward I"",
        ""bed"": ""14"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P016"",
                ""orderitemname"": ""Package 16"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P017"",
                ""orderitemname"": ""Package 17"",
                ""orderqty"": 4
            }
        ]
    },
    {
        ""prescriptionno"": ""1901-10"",
        ""hn"": ""1901"",
        ""an"": ""10"",
        ""patientname"": ""นาง TEST19 TEST20"",
        ""ward"": ""Ward J"",
        ""bed"": ""6"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P018"",
                ""orderitemname"": ""Package 18"",
                ""orderqty"": 1
            }
        ]
    },
    {
        ""prescriptionno"": ""1902-11"",
        ""hn"": ""1902"",
        ""an"": ""11"",
        ""patientname"": ""นาย TEST21 TEST22"",
        ""ward"": ""Ward K"",
        ""bed"": ""11"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P019"",
                ""orderitemname"": ""Package 19"",
                ""orderqty"": 2
            },
            {
                ""orderitemcode"": ""P020"",
                ""orderitemname"": ""Package 20"",
                ""orderqty"": 1
            }
        ]
    },
    {
        ""prescriptionno"": ""1903-12"",
        ""hn"": ""1903"",
        ""an"": ""12"",
        ""patientname"": ""นางสาว TEST23 TEST24"",
        ""ward"": ""Ward L"",
        ""bed"": ""17"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P021"",
                ""orderitemname"": ""Package 21"",
                ""orderqty"": 3
            }
        ]
    },
    {
        ""prescriptionno"": ""1904-13"",
        ""hn"": ""1904"",
        ""an"": ""13"",
        ""patientname"": ""นาย TEST25 TEST26"",
        ""ward"": ""Ward M"",
        ""bed"": ""20"",
        ""status"": ""รอจัด"",
        ""package"": [
            {
                ""orderitemcode"": ""P022"",
                ""orderitemname"": ""Package 22"",
                ""orderqty"": 1
            },
            {
                ""orderitemcode"": ""P023"",
                ""orderitemname"": ""Package 23"",
                ""orderqty"": 2
            },
            {
                ""orderitemcode"": ""P024"",
                ""orderitemname"": ""Package 24"",
                ""orderqty"": 1
            }
        ]
    }
]";

            allPrescriptions = JsonConvert.DeserializeObject<List<Prescription>>(jsonData);
            filteredPrescriptions = allPrescriptions.ToList();
            DisplayPrescriptions();
        }

        private void UpdateStatistics()
        {
            int pendingCount = filteredPrescriptions.Count(p => p.Status == "รอจัด");
            int completedCount = filteredPrescriptions.Count(p => p.Status == "เสร็จแล้ว");
            int totalCount = filteredPrescriptions.Count;

            PendingCountText.Text = pendingCount.ToString();
            CompletedCountText.Text = completedCount.ToString();
            TotalCountText.Text = totalCount.ToString();
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
                Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                FontWeight = FontWeights.Bold
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
                FontWeight = FontWeights.Bold
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

        private void PrintPrescription(Prescription prescription)
        {
            MessageBox.Show($"พิมพ์ใบสั่งยาหมายเลข: {prescription.PrescriptionNo}\nผู้ป่วย: {prescription.PatientName}",
                          "Print Prescription", MessageBoxButton.OK, MessageBoxImage.Information);
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

    // Helper class for ScrollViewer animation
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
        




        //private void Rx_OnTriggerReceived(string data)
        //{
        //    // Dispatcher.Invoke จะเรียกโค้ดนี้บน UI thread
        //    Dispatcher.Invoke(() =>
        //    {
        //        MessageBox.Show("Received: " + data);
        //    });
        //}


        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    // Test button
        //    MessageBox.Show("Button clicked");
        //}

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Button clicked");
        //}
    }
}
