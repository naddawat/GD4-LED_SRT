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
            _var.SerialCan.SetLED(1,Convert.ToInt32(txtAddr.Text), 255, 0, 0);
        }
    }
}
