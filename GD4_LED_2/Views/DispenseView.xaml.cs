using System.IO.Pipes;
using System.IO;
using System.Windows.Controls;

namespace GD4_LED_2.Views
{
    /// <summary>
    /// Interaction logic for DispenseView.xaml
    /// </summary>
    public partial class DispenseView : UserControl
    {
        public DispenseView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var client = new NamedPipeClientStream(".", "wpf_pipe", PipeDirection.Out);
            client.Connect();

            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine("HELLO WPF");

        }
    }
}
