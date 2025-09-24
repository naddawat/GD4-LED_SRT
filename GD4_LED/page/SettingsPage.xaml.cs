using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GD4_LED.page
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private ObservableCollection<LedCabinetModel> ledCabinets;
        private int activeTabIndex = 0;
        public SettingsPage()
        {
            InitializeComponent();
            SetupSmoothScrolling();
            InitializeLedCabinets();
            LoadSettings();

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
            // Load existing settings from configuration
            AutoPrintToggle.IsChecked = true;
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
    }
    public class LedCabinetModel
    {
        public string ID { get; set; }
        public string Addr { get; set; }
        public string ImagePath { get; set; }
        public string ImageInfo { get; set; } = "ไม่มีรูปภาพ";
    }
}
