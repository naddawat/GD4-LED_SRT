using GD4_LED_2.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace GD4_LED_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Optimized version with MVVM pattern - ported from GD4_LED
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // TODO: อ่าน connection string จาก config file หรือ settings
            string connectionString = "Server=localhost;Database=gd4_led;Uid=root;Pwd=;charset=utf8mb4;";

            _viewModel = new MainViewModel(connectionString);
            this.DataContext = _viewModel;

            // Initialize window behaviors from GD4_LED
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set window to secondary screen if available (from GD4_LED)
            SetWindowToSecondaryScreen();

            // Initialize view model
            _ = _viewModel.InitializeAsync();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Confirm before closing
            var result = MessageBox.Show(
                "คุณต้องการออกจากโปรแกรมหรือไม่?",
                "ยืนยันการออก",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            // Cleanup
            _viewModel?.Dispose();
        }

        /// <summary>
        /// ย้าย Window ไปจอที่ 2 ถ้ามี (ported from GD4_LED)
        /// </summary>
        private void SetWindowToSecondaryScreen()
        {
            try
            {
                // ตรวจสอบว่ามีหลายจอหรือไม่
                if (SystemParameters.VirtualScreenWidth > SystemParameters.PrimaryScreenWidth ||
                    SystemParameters.VirtualScreenHeight > SystemParameters.PrimaryScreenHeight)
                {
                    // มีจอหลายจอ - ย้ายไปจอที่ 2
                    this.WindowState = WindowState.Normal;
                    
                    // ตั้งตำแหน่งไปทางขวาของจอหลัก (จอที่ 2)
                    this.Left = SystemParameters.PrimaryScreenWidth;
                    this.Top = 0;
                    
                    // Maximize on secondary screen
                    this.WindowState = WindowState.Maximized;
                }
            }
            catch (Exception ex)
            {
                // แสดงข้อความเตือนถ้าเกิดข้อผิดพลาด
                MessageBox.Show(
                    $"เกิดข้อผิดพลาดในการตั้งค่าจอแสดงผล: {ex.Message}",
                    "ข้อผิดพลาด",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handle header drag to move window (ported from GD4_LED)
        /// </summary>
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2)
                {
                    // Double click to toggle maximize/normal
                    ToggleMaximize();
                }
                else if (e.ButtonState == MouseButtonState.Pressed)
                {
                    // Single click to drag
                    if (this.WindowState == WindowState.Maximized)
                    {
                        this.WindowState = WindowState.Normal;
                        
                        // ปรับตำแหน่งให้อยู่ตรงกลาง title bar
                        var mousePos = PointToScreen(Mouse.GetPosition(this));
                        this.Left = mousePos.X - (this.Width / 2);
                        this.Top = mousePos.Y - 40;
                    }
                    
                    this.DragMove();
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore - can happen during drag
            }
        }

        /// <summary>
        /// Toggle between Maximized and Normal state
        /// </summary>
        private void ToggleMaximize()
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
    }
}
