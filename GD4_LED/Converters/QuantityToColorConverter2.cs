using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GD4_LED
{
    public class QuantityToColorConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int quantity)
            {
                if (quantity <= 20)
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // แดง
                else if (quantity <= 100)
                    return new SolidColorBrush(Color.FromRgb(241, 196, 15)); // เหลือง
                else
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113)); // เขียว
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
