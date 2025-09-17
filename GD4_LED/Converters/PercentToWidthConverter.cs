using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GD4_LED
{
    // Converter ที่แก้ไขแล้ว - ใช้ MultiValueConverter แทน
    public class PercentToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                // คืนค่าเป็น percentage (0-100) สำหรับใช้ใน RelativeSource binding
                return Math.Max(1, percent); // อย่างน้อย 1% เพื่อให้มองเห็น
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

