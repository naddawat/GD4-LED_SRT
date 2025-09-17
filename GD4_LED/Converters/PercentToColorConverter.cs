using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

// Converter สำหรับเปลี่ยนสี Progress Bar ตาม Percent
namespace GD4_LED
{
    public class PercentToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                // แดง (0-25%), เหลือง (26-60%), เขียว (61-100%)
                if (percent <= 25)
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // แดง
                else if (percent <= 60)
                    return new SolidColorBrush(Color.FromRgb(241, 196, 15)); // เหลือง
                else
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113)); // เขียว
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

//// Converter สำหรับคำนวณความกว้างของ Progress Bar
//public class PercentToWidthConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is double percent && parameter is FrameworkElement parent)
//        {
//            return (percent / 100.0) * parent.ActualWidth;
//        }
//        return 0.0;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}

//// Converter สำหรับเปรียบเทียบค่า (ใช้สำหรับเปลี่ยนสีข้อความ)
//public class LessThanConverter : IValueConverter
//{
//    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        if (value is double doubleValue && parameter is string stringParam && double.TryParse(stringParam, out double threshold))
//        {
//            return doubleValue < threshold;
//        }
//        return false;
//    }

//    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//    {
//        throw new NotImplementedException();
//    }
//}