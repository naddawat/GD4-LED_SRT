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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GD4_LED.cls;
using GD4_LED.page;

namespace GD4_LED
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        cls.clsvariable _var = new cls.clsvariable();
        public MainWindow()
        {
            InitializeComponent();
            
            _var.SerialCan = new ClsSubSerial();
            _var.SerialCan.init("COM3");
        }

        private void bnt_open_Click(object sender, RoutedEventArgs e)
        {
            //_var.SerialCan.SetLED(1, Convert.ToInt32(txtAddr.Text), 255, 0, 0);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _var.SerialCan.SetLED(1, Convert.ToInt32(1), 255, 0, 0);

            SetWindowToSecondaryScreen();
            SetActiveTab(DispenseButton);
            MainFrame.Navigate(new DispensePage());
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(StockButton);
            MainFrame.Navigate(new StockWindow());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SetActiveTab(DispenseButton);
            MainFrame.Navigate(new DispensePage());
        }


        // ฟังก์ชันสำหรับย้าย Window ไปจอที่ 2
        private void SetWindowToSecondaryScreen()
        {
            try
            {
                // ใช้ SystemParameters เพื่อตรวจสอบจอหลายจอ
                if (SystemParameters.VirtualScreenWidth > SystemParameters.PrimaryScreenWidth ||
                    SystemParameters.VirtualScreenHeight > SystemParameters.PrimaryScreenHeight)
                {
                    // มีจอหลายจอ - ย้ายไปจอที่ 2
                    this.WindowState = WindowState.Normal;

                    // ตั้งตำแหน่งไปทางขวาของจอหลัก (จอที่ 2)
                    this.Left = SystemParameters.PrimaryScreenWidth;
                    this.Top = 0;
                    this.Width = SystemParameters.PrimaryScreenWidth;
                    this.Height = SystemParameters.PrimaryScreenHeight;

                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    // มีแค่จอเดียว
                    MessageBox.Show(
                        "ระบบตรวจพบจอเดียว โปรแกรมจะแสดงที่จอหลัก",
                        "แจ้งเตือน",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการตั้งค่าจอแสดงผล: {ex.Message}",
                    "ข้อผิดพลาด",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Event สำหรับลาก Window
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    // ถ้าเป็น Maximized ให้เปลี่ยนเป็น Normal ก่อนลาก
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;

                        // ปรับตำแหน่งให้อยู่ตรงกลาง title bar
                        var mousePos = PointToScreen(Mouse.GetPosition(this));
                        this.Left = mousePos.X - (this.Width / 2);
                        this.Top = mousePos.Y - 40;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }

                    this.DragMove();
                }
            }
            catch (InvalidOperationException)
            {
                // จัดการกรณีที่ลากไม่ได้
            }
        }
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    // ถ้าเป็น Maximized ให้เปลี่ยนเป็น Normal ก่อนลาก
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;

                        // ปรับตำแหน่งให้อยู่ตรงกลาง title bar
                        var mousePos = PointToScreen(Mouse.GetPosition(this));
                        this.Left = mousePos.X - (this.Width / 2);
                        this.Top = mousePos.Y - 40;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }

                    this.DragMove();
                }
            }
            catch (InvalidOperationException)
            {
                // จัดการกรณีที่ลากไม่ได้
            }
        }

        // Event สำหรับ Double Click ที่ Header เพื่อ Toggle Maximize/Normal
        private void Header_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // Event สำหรับปิดโปรแกรม
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "คุณต้องการออกจากโปรแกรมหรือไม่?",
                "ยืนยันการออก",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void StockButton_Click(object sender, RoutedEventArgs e)
        {
            //SetActiveTab(StockButton);
            //MainFrame.Navigate(new StockWindow());
            // ถ้าต้องการปิด MainWindow ด้วย ให้ใช้ this.Close();
        }

        private void SetActiveTab(Button activeButton)
        {
            DispenseButton.Tag = null;
            HistoryButton.Tag = null;
            StockButton.Tag = null;

            activeButton.Tag = "Active";
        }



    }
}
