using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GD4_LED_2.Converters
{
    /// <summary>
    /// Converter สำหรับแปลง bool เป็น Color (สำหรับ database status)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับแปลง Loading status เป็น Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter กลับด้าน BoolToVisibility
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับแปลง percentage เป็น Color
    /// </summary>
    public class PercentToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (percentage <= 20)
                    return new SolidColorBrush(Colors.Red);
                else if (percentage <= 50)
                    return new SolidColorBrush(Colors.Orange);
                else if (percentage <= 80)
                    return new SolidColorBrush(Colors.Yellow);
                else
                    return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับแปลง quantity เป็น Color
    /// </summary>
    public class QuantityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int quantity)
            {
                if (quantity <= 0)
                    return new SolidColorBrush(Colors.Red);
                else if (quantity <= 10)
                    return new SolidColorBrush(Colors.Orange);
                else
                    return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับแปลง performance level เป็น Color
    /// </summary>
    public class PerformanceLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorName)
            {
                return colorName.ToLower() switch
                {
                    "red" => new SolidColorBrush(Colors.Red),
                    "yellow" => new SolidColorBrush(Colors.Yellow),
                    "white" => new SolidColorBrush(Colors.White),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับสีข้อความ Text ตามเปอร์เซ็นต์
    /// </summary>
    public class ProgressTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent)
            {
                if (percent <= 25)
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60));
                else if (percent <= 60)
                    return new SolidColorBrush(Color.FromRgb(243, 156, 18));
                else
                    return new SolidColorBrush(Color.FromRgb(46, 204, 113));
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับเปรียบเทียบค่าว่าน้อยกว่าหรือไม่
    /// </summary>
    public class LessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter != null)
            {
                if (double.TryParse(parameter.ToString(), out double threshold))
                {
                    return doubleValue < threshold;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับคำนวณความกว้างของ Progress Bar
    /// </summary>
    public class PercentToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percent && parameter != null)
            {
                if (double.TryParse(parameter.ToString(), out double maxWidth))
                {
                    return Math.Max(2, (percent / 100.0) * maxWidth);
                }
            }
            return 2.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter สำหรับแปลง String Visibility
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            bool hasValue = !string.IsNullOrWhiteSpace(strValue);

            // ถ้ามี parameter = "Inverse" จะกลับค่า
            bool inverse = parameter?.ToString()?.ToLower() == "inverse";
            bool result = inverse ? !hasValue : hasValue;

            return result ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
