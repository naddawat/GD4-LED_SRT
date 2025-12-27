using System;
using System.Configuration;
using System.Threading;
using System.Windows;

namespace GD4_LED_2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // ป้องกันเปิดโปรแกรมซ้ำ
            const string appName = "GD4_LED_2_SingleInstance";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("โปรแกรมถูกเปิดอยู่แล้ว",
                                "แจ้งเตือน",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                Shutdown();
                return;
            }

            base.OnStartup(e);

            // Handle unhandled exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"เกิดข้อผิดพลาดที่ไม่คาดคิด:\n{e.Exception.Message}",
                          "ข้อผิดพลาด",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);

            // Log the exception
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.Exception}");

            e.Handled = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
