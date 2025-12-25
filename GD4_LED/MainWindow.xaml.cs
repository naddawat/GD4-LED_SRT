using GD4_LED.cls;
using GD4_LED.page;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Reflection;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GD4_LED
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        clsMain _Man = new clsMain();
        clsQuery _STK = new clsQuery();
        clsutilDB _con = new clsutilDB();
        clsConfig _config = new clsConfig();
        bool v = false;
        bool local = false;
        public MainWindow()
        {
            InitializeComponent();

            clsvariable.comname = Environment.MachineName;
            if(clsvariable.comname == "HP")
            {
                clsvariable.comname = "GD4-LED-1";
            }
            
            v = _STK.CheckConnection(GD4_LED.Properties.Settings.Default.connectstring);
            if(!v)
            {
                MessageBox.Show(" ไม่สามารถเชื่อมฐานข้อมูล : " + GD4_LED.Properties.Settings.Default.connectstring);
            }

           
            string version = "";          

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                // ถ้ายังไม่ publish ให้ใช้ assembly version แทน
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            txtversion.Text = "Medicine Management System | ver : " + version + " comname : "+ clsvariable.comname;

            // สร้าง SerialCan แค่ครั้งเดียว
            if (clsvariable.Instance.SerialCan == null)
            {
                clsvariable.Instance.SerialCan = new ClsSubSerial();
                clsvariable.Instance.SerialCan.init("COM3");
            }
            //_var.SerialCan = new ClsSubSerial();
            //_var.SerialCan.init("COM3");
        }

        private void bnt_open_Click(object sender, RoutedEventArgs e)
        {
            //_var.SerialCan.SetLED(1, Convert.ToInt32(txtAddr.Text), 255, 0, 0);
        }
      
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //_var.SerialCan.SetLED(1, Convert.ToInt32(1), 255, 0, 0);
            
            string datetimeNow = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            SetWindowToSecondaryScreen();
            SetActiveTab(DispenseButton);
            MainFrame.Navigate(new DispensePage());
            txtdevice.Text = _Man.getDeviceDetail(clsvariable.comname);
            txtdatetime.Text = datetimeNow;

            clsvariable.dt_LedConfig = _config.GetLedConfig(clsvariable.comname);
            if (clsvariable.dt_LedConfig.Rows.Count > 0)
            {
                clsvariable.RGD_dispense = clsvariable.dt_LedConfig.Rows[0]["RGB_dispense"].ToString().Split('|');
                clsvariable.comport = clsvariable.dt_LedConfig.Rows[0]["serial_port"].ToString();
                clsvariable.crp_report = clsvariable.dt_LedConfig.Rows[0]["crp_report"].ToString();
                clsvariable.printname = clsvariable.dt_LedConfig.Rows[0]["printname"].ToString();
                //MessageBox.Show(clsvariable.printname);
                if (clsvariable.dt_LedConfig.Rows[0]["print_isenable"].ToString() == "Y")
                {
                    clsvariable.print_isenable = true;
                }
                else
                {
                    clsvariable.print_isenable = false;
                }

                if (clsvariable.dt_LedConfig.Rows[0]["trigger_isenable"].ToString() == "Y")
                {
                    clsvariable.trigger_isenable = true;
                }
                else
                {
                    clsvariable.trigger_isenable = false;
                }

                clsvariable.sever = clsvariable.dt_LedConfig.Rows[0]["server"].ToString();
                clsvariable.database = clsvariable.dt_LedConfig.Rows[0]["database"].ToString();
                clsvariable.port = clsvariable.dt_LedConfig.Rows[0]["port"].ToString();
                clsvariable.username = clsvariable.dt_LedConfig.Rows[0]["username"].ToString();
                clsvariable.password = clsvariable.dt_LedConfig.Rows[0]["password"].ToString();

                if(clsvariable.sever != "" && clsvariable.database != "" && clsvariable.username != "" && clsvariable.password != "")
                {
                    clsvariable.connectionST = $@"Data Source={clsvariable.sever};Initial Catalog={clsvariable.database};Persist Security Info=True;User ID={clsvariable.username};Password={clsvariable.password}; charset=utf8mb4;";
                }
            }

            local = _STK.CheckConnection(clsvariable.connectionST);
            if (!local)
            {
                MessageBox.Show(" ไม่สามารถเชื่อมฐานข้อมูล : " + clsvariable.connectionST);
            }
            clsvariable.dt_Ledinfo = _STK.GetLedInfo(clsvariable.comname);
            if (clsvariable.dt_Ledinfo.Rows.Count > 0)
            {
                //txtdevice.Text = clsvariable.dt_Ledinfo.Rows[0]["shelfzone"].ToString();
                clsvariable.shelfzone = clsvariable.dt_Ledinfo.Rows[0]["shelfzone"].ToString();
            }

            if (v&& local) // method ที่คุณเขียนไว้เช็คการต่อ
            {
                myEllipse.Fill = (Brush)FindResource("Success"); // สีเขียว
                myEllipse.ToolTip = "ระบบออนไลน์";
            }
            else
            {
                myEllipse.Fill = Brushes.Red;
                myEllipse.ToolTip = "เชื่อมต่อ Database ไม่ได้";
            }
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
                    //// มีแค่จอเดียว
                    //MessageBox.Show(
                    //    "ระบบตรวจพบจอเดียว โปรแกรมจะแสดงที่จอหลัก",
                    //    "แจ้งเตือน",
                    //    MessageBoxButton.OK,
                    //    MessageBoxImage.Information);
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
            SettingsButton.Tag = null;

            activeButton.Tag = "Active";
        }

        private void StockButton_Click_1(object sender, RoutedEventArgs e)
        {
            SetActiveTab(StockButton);
            MainFrame.Navigate(new StockWindow());
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(HistoryButton);
            MainFrame.Navigate(new DispensePageHistory());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {   
            SetActiveTab(SettingsButton);
            MainFrame.Navigate(new SettingsPage());
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");

        }
    }
}
