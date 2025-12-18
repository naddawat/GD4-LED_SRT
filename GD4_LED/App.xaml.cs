using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GD4_LED
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "MyWpfSingleInstanceApp";
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
        }

    }
}
