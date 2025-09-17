using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

// Converter สำหรับเปลี่ยนสี Progress Bar ตาม Percent
//// Converter สำหรับเปรียบเทียบค่า (ใช้สำหรับเปลี่ยนสีข้อความ)
namespace GD4_LED
{
    public class LessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string stringParam && double.TryParse(stringParam, out double threshold))
            {
                return doubleValue < threshold;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}