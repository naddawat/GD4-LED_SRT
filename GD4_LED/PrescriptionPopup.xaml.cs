using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace GD4_LED
{
    /// <summary>
    /// Interaction logic for PrescriptionPopup.xaml
    /// </summary>
    public partial class PrescriptionPopup : Window
    {
        private HashSet<PackageItem> selectedItems = new HashSet<PackageItem>();
        private PrescriptionData prescriptionData;
        public PrescriptionPopup(string jsonString)
        {
            InitializeComponent();
            LoadSampleData(jsonString);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadSampleData(string jsonString)
        {
            // Sample JSON data
            string jsonData = @"{
                ""PrescriptionNo"": ""681210464"",
                ""HN"": ""1383699"",
                ""AN"": ""68073055"",
                ""PatientName"": ""น.ส.โฉมสุดา เพิงขุนทด"",
                ""Ward"": ""ICU Neuro"",
                ""Bed"": ""07"",
                ""Status"": ""รอจัด"",
                ""Package"": [
                    {
                        ""OrderItemCode"": ""LOSECL"",
                        ""OrderItemName"": ""OMEPRAZOLE 40MG INJ (LOSEC)L"",
                        ""OrderQty"": 1
                    },
                    {
                        ""OrderItemCode"": ""HAE6"",
                        ""OrderItemName"": ""6% HAE-STERIL 500 ML (Voluven)"",
                        ""OrderQty"": 4
                    }
                ]
            }";

            jsonData = jsonString;

            LoadData(jsonData);
        }

        public void LoadData(string jsonData)
        {
            prescriptionData = JsonConvert.DeserializeObject<PrescriptionData>(jsonData);

            // Set header information
            txtPrescriptionNo.Text = $"เลขที่ใบสั่ง: {prescriptionData.PrescriptionNo}";
            txtPatientName.Text = prescriptionData.PatientName;
            txtHN.Text = prescriptionData.HN;
            txtAN.Text = prescriptionData.AN;
            txtWard.Text = prescriptionData.Ward;
            txtBed.Text = prescriptionData.Bed;
            txtStatus.Text = prescriptionData.Status;

            // Set status color
            SetStatusColor(prescriptionData.Status);

            // Bind package items
            PackageItemsControl.ItemsSource = prescriptionData.Package;

            // Update footer
            UpdateFooter();
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

        private void PackageCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;

            var item = border.Tag as PackageItem;
            if (item == null) return;

            // Find the checkMark border
            var grid = border.Child as Grid;
            if (grid == null) return;

            var checkMark = FindVisualChild<Border>(grid, "checkMark");
            if (checkMark == null) return;

            // Toggle selection
            if (selectedItems.Contains(item))
            {
                // Unselect
                selectedItems.Remove(item);
                AnimateCheckMark(checkMark, border, false);
            }
            else
            {
                // Select
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

                // Animate background to green
                var colorAnimation = new ColorAnimation
                {
                    To = (Color)ColorConverter.ConvertFromString("#10b981"),
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                checkMark.Background = new SolidColorBrush(Colors.Transparent);
                checkMark.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                // Scale animation
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

                // Card border animation
                cardBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10b981"));
                cardBorder.BorderThickness = new Thickness(2);
            }
            else
            {
                // Fade out animation
                var opacityAnimation = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };
                opacityAnimation.Completed += (s, e) => checkMark.Visibility = Visibility.Collapsed;
                checkMark.BeginAnimation(Border.OpacityProperty, opacityAnimation);

                // Reset card border
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

        // Method to get selected items (สำหรับเรียกใช้จาก Window อื่น)
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
