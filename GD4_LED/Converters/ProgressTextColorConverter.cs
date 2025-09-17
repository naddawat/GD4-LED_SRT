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
    // Converter สำหรับเปลี่ยนสีข้อความใน Progress Bar ให้มองเห็นชัด
    public class ProgressTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                // สีข้อความที่มองเห็นชัดบนพื้นหลังแต่ละสี
                if (percent <= 25)
                    return new SolidColorBrush(Colors.Red); // ข้อความสีขาวบนพื้นแดง
                else if (percent <= 60)
                    return new SolidColorBrush(Colors.Black); // ข้อความสีดำบนพื้นเหลือง
                else
                    return new SolidColorBrush(Colors.White); // ข้อความสีขาวบนพื้นเขียว
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
