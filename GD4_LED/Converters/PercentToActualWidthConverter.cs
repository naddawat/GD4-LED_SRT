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
    public class PercentToActualWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2 &&
                values[0] is double percent &&
                values[1] is double containerWidth &&
                containerWidth > 0)
            {
                return Math.Max(2, (percent / 100.0) * containerWidth);
            }
            return 2.0; // ความกว้างขั้นต่ำ
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
