using GD4_LED.cls;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Mysqlx.Expr;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

//using System.Drawing.Printing;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Color = System.Windows.Media.Color;

namespace GD4_LED.page
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private ObservableCollection<LedCabinetModel> ledCabinets;
        private int activeTabIndex = 0;
        clsConfig _config = new clsConfig();
        clsQuery _query = new clsQuery();   
        public SettingsPage()
        {
            InitializeComponent();
            SetupSmoothScrolling();
            InitializeLedCabinets();
            LoadSettings();
            LoadSerialPorts();
            LoadPrinters();
            
        }

        private void LoadPrinters()
        {
            //CbPrinter.Items.Clear();

            //foreach (string printerName in PrinterSettings.InstalledPrinters)
            //{
            //    bool isOnline = IsPrinterOnline(printerName);
            //    string displayName = isOnline ? $"{printerName} " : printerName;

            //    CbPrinter.Items.Add(displayName);
            //}

            //if (CbPrinter.Items.Count > 0)
            //{
            //    TbPrinter.Text = "";
            //    CbPrinter.SelectedIndex = 0;
            //}
            //else
            //{
            //    TbPrinter.Text = "Printer";
            //}
        }

        private bool IsPrinterOnline(string printerName)
        {
            //try
            //{
            //    PrinterSettings ps = new PrinterSettings();
            //    ps.PrinterName = printerName;

            //    // ถ้าเครื่องพิมพ์นี้ไม่รองรับ ตรวจสอบได้ด้วย IsValid
            //    if (!ps.IsValid)
            //        return false;

            //    // ตรวจสอบสถานะ Printer ผ่าน PrinterSettings
            //    // Note: ถ้าอยากละเอียด ต้องใช้ WMI หรือ System.Printing
            //    return true; // ถือว่าออนไลน์ / ใช้งานได้
            //}
            //catch
            //{
            return false;
            //}
        }

        private void LoadSerialPorts()
        {
            CbSerialPort.Items.Clear();

            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                bool isAvailable = IsPortAvailable(port);
                string displayName = isAvailable ? $"{port} " : port;

                CbSerialPort.Items.Add(displayName);
            }

            if (CbSerialPort.Items.Count > 0)
            {
                TbSerialPort.Text = "";
                CbSerialPort.SelectedIndex = 0;
            }
            else
            {
                TbSerialPort.Text = "Serial Port";
            }


        }

        // ตรวจสอบว่า COM port เปิดได้หรือไม่
        private bool IsPortAvailable(string portName)
        {
            try
            {
                using (SerialPort port = new SerialPort(portName))
                {
                    port.Open();
                    port.Close();
                    return true; // เปิดสำเร็จ
                }
            }
            catch
            {
                return false; // ใช้งานไม่ได้
            }
        }
        private void SetButtonContent(string buttonName, string drugName)
        {
            Button button = FindName(buttonName) as Button;
            if (button != null)
            {
                button.Content = $"{buttonName.Substring(buttonName.Length - 3)}\n{drugName}";
                button.HorizontalContentAlignment = HorizontalAlignment.Center;
                button.VerticalContentAlignment = VerticalAlignment.Center;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // การจัดการเมื่อคลิกปุ่ม
                string content = button.Content.ToString();
                // ทำสิ่งที่ต้องการเมื่อคลิกปุ่ม
            }
        }
        private void SetupSmoothScrolling()
        {
            SettingsScrollViewer.ScrollChanged += (sender, e) =>
            {
                if (e.VerticalChange != 0)
                {
                    SettingsScrollViewer.InvalidateVisual();
                }
            };

            SettingsScrollViewer.PreviewMouseWheel += (sender, e) =>
            {
                ScrollViewer scrollViewer = sender as ScrollViewer;
                if (scrollViewer != null)
                {
                    double scrollAmount = e.Delta > 0 ? -120 : 120;
                    var animation = new DoubleAnimation()
                    {
                        From = scrollViewer.VerticalOffset,
                        To = Math.Max(0, Math.Min(scrollViewer.ScrollableHeight, scrollViewer.VerticalOffset + scrollAmount)),
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
                    };

                    scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
                    e.Handled = true;
                }
            };
        }

        private void InitializeLedCabinets()
        {
            ledCabinets = new ObservableCollection<LedCabinetModel>
            {
                new LedCabinetModel { ID = "LED001", Addr = "192.168.1.100" },
                new LedCabinetModel { ID = "LED002", Addr = "192.168.1.101" }
            };

            //CabinetItemsControl.ItemsSource = ledCabinets;
        }

        private void LoadSettings()
        {
            if (clsvariable.dt_LedConfig.Rows.Count <= 0)
            {
                clsvariable.dt_LedConfig = _config.GetLedConfig(clsvariable.comname);
            }
            else
            {
                if(clsvariable.print_isenable)
                {
                    AutoPrintToggle.IsChecked = true;
                }
                else
                {
                    AutoPrintToggle.IsChecked = false;
                }
                if(clsvariable.printname != "")
                {
                    CbPrinter.Items.Add(clsvariable.printname);
                }

                if(clsvariable.comport != "")
                {
                    CbSerialPort.Items.Add(clsvariable.comport);
                }
                if (clsvariable.trigger_isenable)
                {
                    SoundNotificationToggle.IsChecked = true;
                }
                else
                {
                    SoundNotificationToggle.IsChecked = false;
                }

                ServerTextBox.Text = clsvariable.sever;
                DatabaseTextBox.Text = clsvariable.database;
                PortTextBox.Text = clsvariable.port;
                UsernameTextBox.Text = clsvariable.username;
                PasswordBox.Password = clsvariable.password;

            }
            // Load existing settings from configuration
            
            //PrintPrescriptionToggle.IsChecked = false;
            //SoundNotificationToggle.IsChecked = true;
            //DarkModeToggle.IsChecked = false;
            SetButtonContent("BtnLed801", "พาราเซตามอล");
            SetButtonContent("BtnLed802", "อะม็อกซีซิลลิน");
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int tabIndex = int.Parse(clickedButton.Tag.ToString());

            if (tabIndex == activeTabIndex) return;

            // Update tab button styles
            ResetTabStyles();
            clickedButton.Style = (Style)FindResource("ActiveTabButtonStyle");

            // Show/hide tab content with animation
            ShowTabContent(tabIndex);
            activeTabIndex = tabIndex;
        }

        private void ResetTabStyles()
        {
            DatabaseTab.Style = (Style)FindResource("TabButtonStyle");
            LedCabinetTab.Style = (Style)FindResource("TabButtonStyle");
            GeneralTab.Style = (Style)FindResource("TabButtonStyle");
        }

        private void ShowTabContent(int tabIndex)
        {
            // Hide all content
            DatabaseContent.Visibility = Visibility.Collapsed;
            LedCabinetContent.Visibility = Visibility.Collapsed;
            GeneralContent.Visibility = Visibility.Collapsed;

            // Show selected content with fade in animation
            Border targetContent = null;
            switch (tabIndex)
            {
                case 0: targetContent = DatabaseContent; break;
                case 1: targetContent = LedCabinetContent; break;
                case 2: targetContent = GeneralContent; break;
            }

            if (targetContent != null)
            {
                targetContent.Visibility = Visibility.Visible;
                targetContent.Opacity = 0;

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                targetContent.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        // Database Tab Events
        private void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("กำลังทดสอบการเชื่อมต่อ...", "Database Connection",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            // Simulate connection test
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("เชื่อมต่อฐานข้อมูลสำเร็จ!", "Connection Test",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }

        private void SaveDatabase_Click(object sender, RoutedEventArgs e)
        {
            // Save database configuration
            MessageBox.Show("บันทึกการตั้งค่าฐานข้อมูลแล้ว", "Settings Saved",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // LED Cabinet Tab Events
        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LedCabinetModel cabinet = button.Tag as LedCabinetModel;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JPEG Image|*.jpg;*.jpeg",
                Title = "เลือกรูปภาพสำหรับตู้ LED"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Validate image
                if (ValidateImage(filePath))
                {
                    cabinet.ImagePath = filePath;
                    cabinet.ImageInfo = GetImageInfo(filePath);

                    // Update the UI
                    //CabinetItemsControl.ItemsSource = null;
                    //CabinetItemsControl.ItemsSource = ledCabinets;
                }
            }
        }

        private bool ValidateImage(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // Check file size (max 4KB)
                if (fileInfo.Length > 4096)
                {
                    MessageBox.Show("ขนาดไฟล์ต้องไม่เกิน 4KB", "Image Validation",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Check image dimensions
                var image = new BitmapImage(new Uri(filePath));

                if (image.PixelWidth > 120 || image.PixelHeight > 120)
                {
                    MessageBox.Show("ขนาดรูปภาพต้องไม่เกิน 120x120 พิกเซล",
                                    "Image Validation",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return false;
                }


                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ไม่สามารถอ่านไฟล์รูปภาพได้: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private string GetImageInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                var image = new BitmapImage(new Uri(filePath));
                return $"{image.PixelWidth}x{image.PixelHeight}px, {fileInfo.Length / 1024.0:F1}KB";


            }
            catch
            {
                return "ข้อมูลไม่ถูกต้อง";
            }
        }

        private void SaveCabinet_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LedCabinetModel cabinet = button.Tag as LedCabinetModel;

            MessageBox.Show($"บันทึกการตั้งค่าตู้ LED {cabinet.ID} แล้ว", "Settings Saved",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteCabinet_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            LedCabinetModel cabinet = button.Tag as LedCabinetModel;

            var result = MessageBox.Show($"ต้องการลบตู้ LED {cabinet.ID} หรือไม่?", "Confirm Delete",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ledCabinets.Remove(cabinet);
                MessageBox.Show("ลบตู้ LED แล้ว", "Deleted",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddCabinet_Click(object sender, RoutedEventArgs e)
        {
            var newCabinet = new LedCabinetModel
            {
                ID = $"LED{ledCabinets.Count + 1:000}",
                Addr = "192.168.1." + (100 + ledCabinets.Count + 1)
            };

            ledCabinets.Add(newCabinet);
        }

        // General Settings Tab Events
        private void SaveGeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            // Save all general settings
            bool autoPrint = AutoPrintToggle.IsChecked ?? false;
            bool autoTrig = SoundNotificationToggle.IsChecked ?? false;
            //bool printPrescription = PrintPrescriptionToggle.IsChecked ?? false;
            //bool soundNotification = SoundNotificationToggle.IsChecked ?? false;
            //bool darkMode = DarkModeToggle.IsChecked ?? false;

            // Apply dark mode if changed
            //if (darkMode)
            //{
            //    // Apply dark theme logic here
            //}

            MessageBox.Show("บันทึกการตั้งค่าทั่วไปแล้ว", "Settings Saved",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            ColorPicker(clickedButton);
        }

        private void ColorPicker(Button clickedButton)
        {
            //Button clickedButton = sender as Button;

            //Button clickedButton = new Button();

            var colorPickerDialog = new Window
            {
                Title = "เลือกสี",
                Width = 450,
                Height = 850,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Background = new SolidColorBrush(Color.FromRgb(245, 245, 250))
            };

            var mainPanel = new StackPanel { Margin = new Thickness(20) };

            // ========== หมวดสีพื้นฐาน ==========
            var basicColorSection = CreateSection("สีพื้นฐาน");

            var basicColorsGrid = new UniformGrid
            {
                Columns = 6,
                Margin = new Thickness(0, 10, 0, 0)
            };

                    var basicColors = new[]
                    {
                new { Color = Colors.Red, Name = "แดง" },
                new { Color = Colors.Orange, Name = "ส้ม" },
                new { Color = Colors.Yellow, Name = "เหลือง" },
                new { Color = Colors.Green, Name = "เขียว" },
                new { Color = Colors.Blue, Name = "น้ำเงิน" },
                new { Color = Colors.Purple, Name = "ม่วง" },
                new { Color = Colors.Pink, Name = "ชมพู" },
                new { Color = Colors.Brown, Name = "น้ำตาล" },
                new { Color = Colors.Cyan, Name = "ฟ้า" },
                new { Color = Colors.Black, Name = "ดำ" },
                new { Color = Colors.Gray, Name = "เทา" },
                new { Color = Colors.White, Name = "ขาว" }
            };

            foreach (var colorInfo in basicColors)
            {
                var colorButton = new Button
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5),
                    Background = new SolidColorBrush(colorInfo.Color),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    BorderThickness = new Thickness(2),
                    Tag = colorInfo.Color,
                    ToolTip = colorInfo.Name,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Style = CreateColorButtonStyle()
                };

                colorButton.Click += (s, args) =>
                {
                    clickedButton.Background = new SolidColorBrush((Color)colorButton.Tag);
                    colorPickerDialog.Close();
                };

                basicColorsGrid.Children.Add(colorButton);
            }

            basicColorSection.Children.Add(basicColorsGrid);
            mainPanel.Children.Add(basicColorSection);

            // ========== หมวดกำหนดสีเอง ==========
            var customColorSection = CreateSection("กำหนดสีเอง");
            customColorSection.Margin = new Thickness(0, 20, 0, 0);
            var initialColor = ((SolidColorBrush)clickedButton.Background).Color;

            var redSlider = CreateModernColorSlider("แดง (R)", Colors.Red, initialColor.R);
            var greenSlider = CreateModernColorSlider("เขียว (G)", Colors.Green, initialColor.G);
            var blueSlider = CreateModernColorSlider("น้ำเงิน (B)", Colors.Blue, initialColor.B);

            customColorSection.Children.Add((UIElement)redSlider.Tag);
            customColorSection.Children.Add((UIElement)greenSlider.Tag);
            customColorSection.Children.Add((UIElement)blueSlider.Tag);

            mainPanel.Children.Add(customColorSection);

            // ========== แสดงตัวอย่างสี ==========
            var previewSection = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            previewSection.Children.Add(new TextBlock
            {
                Text = "ตัวอย่างสี",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            var previewBorder = new Border
            {
                Width = 150,
                Height = 60,
                BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                BorderThickness = new Thickness(2),
                Background = new SolidColorBrush(initialColor),
                CornerRadius = new CornerRadius(8),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Gray,
                    Opacity = 0.3,
                    BlurRadius = 10,
                    ShadowDepth = 3
                }
            };
            var currentColor = (clickedButton.Background as SolidColorBrush)?.Color ?? Colors.White;
            var colorCodeText = new TextBlock
            {
                Text = $"#{currentColor.R:X2}{currentColor.G:X2}{currentColor.B:X2}",
                FontSize = 12,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };

            previewSection.Children.Add(previewBorder);
            previewSection.Children.Add(colorCodeText);
            mainPanel.Children.Add(previewSection);

            void UpdatePreview()
            {
                var color = Color.FromRgb(
                    (byte)redSlider.Value,
                    (byte)greenSlider.Value,
                    (byte)blueSlider.Value);
                previewBorder.Background = new SolidColorBrush(color);
                colorCodeText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }

            redSlider.ValueChanged += (s, e1) => UpdatePreview();
            greenSlider.ValueChanged += (s, e2) => UpdatePreview();
            blueSlider.ValueChanged += (s, e3) => UpdatePreview();

            // ========== หมวดทดสอบ ==========
            var testSection = CreateSection("ทดสอบการใช้งาน");
            testSection.Margin = new Thickness(0, 20, 0, 0);

            var testGrid = new Grid { Margin = new Thickness(0, 10, 0, 0) };
            testGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            testGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var rowPanel = new StackPanel { Margin = new Thickness(0, 0, 5, 0) };
            rowPanel.Children.Add(new TextBlock
            {
                Text = "แถว",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 5)
            });

            var rowCombo = new ComboBox
            {
                Style = (Style)FindResource("ModernComboBoxStyle"),
                ItemsSource = Enumerable.Range(1, 8).ToList(),
                SelectedIndex = 0,
                BorderBrush = System.Windows.Media.Brushes.DarkGray,  // สีขอบ
                BorderThickness = new Thickness(2), // ความหนาขอบ
                //CornerRadius = new CornerRadius(5) // ถ้า ModernComboBoxStyle รองรับ
            };

            rowPanel.Children.Add(rowCombo);
            Grid.SetColumn(rowPanel, 0);

            var colPanel = new StackPanel { Margin = new Thickness(5, 0, 0, 0) };
            colPanel.Children.Add(new TextBlock
            {
                Text = "คอลัมน์",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Thickness(0, 0, 0, 5)
            });

            var colCombo = new ComboBox
            {
                Style = (Style)FindResource("ModernComboBoxStyle"),
                ItemsSource = Enumerable.Range(1, 6).ToList(),
                SelectedIndex = 0,
                BorderBrush = System.Windows.Media.Brushes.DarkGray,  // สีขอบ
                BorderThickness = new Thickness(2), // ความหนาขอบ
            };
            colPanel.Children.Add(colCombo);
            Grid.SetColumn(colPanel, 1);

            testGrid.Children.Add(rowPanel);
            testGrid.Children.Add(colPanel);
            testSection.Children.Add(testGrid);

            var toggleContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 10, 0, 0)
            };

            var togglePanel = new StackPanel { Orientation = Orientation.Horizontal };

            var lightToggle = new CheckBox
            {
                Style = (Style)FindResource("ToggleSwitchStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };

            var toggleLabel = new TextBlock
            {
                Text = "เปิด/ปิดไฟ",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            togglePanel.Children.Add(lightToggle);
            togglePanel.Children.Add(toggleLabel);
            toggleContainer.Child = togglePanel;
            testSection.Children.Add(toggleContainer);

            mainPanel.Children.Add(testSection);

            // ========== ปุ่มยืนยัน ==========
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 25, 0, 0)
            };

            var okButton = new Button
            {
                Content = "✓ ตกลง",
                Width = 120,
                Height = 38,
                Margin = new Thickness(5, 0, 5, 0),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(67, 160, 71)),
                Foreground = System.Windows.Media.Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Style = CreateModernButtonStyle()
            };


            // ปรับสีปุ่ม OK ให้บันทึกสีที่เลือก
            okButton.Click += (s, e4) =>
            {
                 MessageBoxResult result = MessageBox.Show("คุณต้องการบันทึกหรือไม่?", "ยืนยัน", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // โค้ดเมื่อกด Yes
                string selectedColor = colorCodeText.Text;
                string hexColor = "#0080FF";
                System.Drawing.Color color = ColorTranslator.FromHtml(hexColor);

                int r = color.R;
                int g = color.G;
                int b = color.B;
                selectedColor = $"{r}|{g}|{b}|";
                clickedButton.Background = previewBorder.Background;
                colorPickerDialog.Close();

            }
                else
            {
                // โค้ดเมื่อกด No
            }

                
            };

            // ปุ่มเปิดไฟ/ปิดไฟ
            lightToggle.Click += (s, e5) =>
            {
                if (lightToggle.IsChecked == true)
                {
                    // Logic to turn on the light
                    //MessageBox.Show("เปิดไฟแล้ว", "Light Control",
                    //MessageBoxButton.OK, MessageBoxImage.Information);
                    int red = (int)redSlider.Value;
                    int green = (int)greenSlider.Value;
                    int blue = (int)blueSlider.Value;
                    for (int i = 0; i < 6; i++)
                    {
                        clsvariable.Instance.SerialCan.SetLED(1, i, red, green, blue);
                    }
                    
                }
                else
                {
                    // Logic to turn off the light
                    //MessageBox.Show("ปิดไฟแล้ว", "Light Control",
                    //              MessageBoxButton.OK, MessageBoxImage.Information);

                    for (int i = 0; i < 6; i++)
                    {
                        clsvariable.Instance.SerialCan.SetLED(1, i, 0, 0, 0);
                    }
                }
            };


            var cancelButton = new Button
            {
                Content = "✕ ยกเลิก",
                Width = 120,
                Height = 38,
                Margin = new Thickness(5, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Style = CreateModernButtonStyle()
            };

            cancelButton.Click += (s, e5) => colorPickerDialog.Close();

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            mainPanel.Children.Add(buttonPanel);

            // ========== แสดง Dialog ==========
            var scrollViewer = new ScrollViewer
            {
                Content = mainPanel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            colorPickerDialog.Content = scrollViewer;
            colorPickerDialog.Owner = Window.GetWindow(this);
            colorPickerDialog.ShowDialog();

           
        }

        // ========== Helper Methods ==========

        private StackPanel CreateSection(string title)
        {
            var section = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(50, 50, 50))
            };

            var separator = new Border
            {
                Height = 2,
                Background = new LinearGradientBrush(
                    Color.FromRgb(100, 150, 255),
                    Color.FromRgb(150, 180, 255),
                    0),
                CornerRadius = new CornerRadius(1),
                Margin = new Thickness(0, 5, 0, 0)
            };

            section.Children.Add(titleBlock);
            section.Children.Add(separator);

            return section;
        }

        private Slider CreateModernColorSlider(string label, Color color, byte initialValue)
        {
            var container = new StackPanel
            {
                Margin = new Thickness(0, 12, 0, 0)
            };

            var labelPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var labelText = new TextBlock
            {
                Text = label,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                VerticalAlignment = VerticalAlignment.Center
            };

            var valueText = new TextBlock
            {
                Text = initialValue.ToString(),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0),
                MinWidth = 30,
                TextAlignment = TextAlignment.Right
            };

            labelPanel.Children.Add(labelText);
            labelPanel.Children.Add(valueText);
            container.Children.Add(labelPanel);

            var slider = new Slider
            {
                Minimum = 0,
                Maximum = 255,
                Value = initialValue,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Foreground = new SolidColorBrush(color)
            };

            slider.ValueChanged += (s, e) =>
            {
                valueText.Text = ((int)e.NewValue).ToString();
            };

            container.Children.Add(slider);
            slider.Tag = container;

            return slider;
        }

        private Style CreateModernButtonStyle()
        {
            var style = new Style(typeof(Button));

            style.Setters.Add(new Setter(Button.TemplateProperty, CreateButtonTemplate()));

            return style;
        }

        private ControlTemplate CreateButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));

            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.PaddingProperty, new Thickness(15, 8, 15, 8));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(contentPresenter);
            template.VisualTree = border;

            return template;
        }

        private Style CreateColorButtonStyle()
        {
            var style = new Style(typeof(Button));

            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.Name = "border";
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            border.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));

            template.VisualTree = border;
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            return style;
        }
        

        private void AutoPrintToggle_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void AutoPrintToggle_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            if (AutoPrintToggle.IsChecked == true)
            {
                clsvariable.print_isenable = true;
                result = _query.InsertLog("ตั้งค่า", "user", "เปิดใช้งานการพิมพ์ฉลากยา");
                if (result)
                {
                    result = false;
                    result = _query.UpdatePrintStatus("Y", clsvariable.comname);
                }
            }
            else if (AutoPrintToggle.IsChecked == false)
            {
                clsvariable.print_isenable = false;
                result = _query.InsertLog("ตั้งค่า", "user", "ปิดใช้งานการพิมพ์ฉลากยา");
                if (result)
                {
                    result = false;
                    result = _query.UpdatePrintStatus("N", clsvariable.comname);
                }
            }

            if (result)
            {
                LoadSettings();
            }
        }

        private void SoundNotificationToggle_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;
            if (SoundNotificationToggle.IsChecked == true)
            {
                clsvariable.print_isenable = true;
                result = _query.InsertLog("ตั้งค่า", "user", "เปิดการรับข้อมูลจาก Server");
                if (result)
                {
                    result = false;
                    result = _query.UpdateTrigger("Y", clsvariable.comname);
                }
            }
            else if (SoundNotificationToggle.IsChecked == false)
            {
                clsvariable.print_isenable = false;
                result = _query.InsertLog("ตั้งค่า", "user", "ปิดการรับข้อมูลจาก Server");
                if (result)
                {
                    result = false;
                    result = _query.UpdateTrigger("N", clsvariable.comname);
                }
            }

            if (result)
            {
                LoadSettings();
            }
        }

        private void SoundNotificationToggle_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoadLocation();
        }
        public bool SyncDrug()
        {
            DataTable dt_stock = new DataTable();
            dt_stock = _query.GetLedStockByZone(clsvariable.shelfzone);
            string connStr = GD4_LED.Properties.Settings.Default.connectstringlocal;
            int position_id =0;
            int addr=0;
            string qty="";
            string LotNo="";
            string exp="";
            string orderitemENname ="";
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

                    clsvariable.Instance.SerialCan.SetEEprom(addr, addr, position_id,orderitemENname,"","", position);
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
        public void LoadLocation()
        {
            DataTable dt = new DataTable();
            dt = _query.GetLocation_main(clsvariable.shelfzone);
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

                    string sql = $@" INSERT INTO ms_location ({insertCols}) VALUES ({insertParams}) ON DUPLICATE KEY UPDATE {updateCols}; ";

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
        public void LoadLocationByCode(string code)
        {
            DataTable dt = new DataTable();
            dt = _query.GetLocation_main(clsvariable.shelfzone);
            string connStr = GD4_LED.Properties.Settings.Default.connectstringlocal;
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

                    string sql = $@" INSERT INTO ms_location ({insertCols}) VALUES ({insertParams}) ON DUPLICATE KEY UPDATE {updateCols}; ";

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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SyncDrug();
        }
    }


    public class LedCabinetModel
    {
        public string ID { get; set; }
        public string Addr { get; set; }
        public string ImagePath { get; set; }
        public string ImageInfo { get; set; } = "ไม่มีรูปภาพ";
    }
}
