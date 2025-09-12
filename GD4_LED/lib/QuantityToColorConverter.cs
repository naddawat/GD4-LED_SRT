using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GD4_LED
{
    public class QuantityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int qty = 0;
            int.TryParse(value?.ToString(), out qty);
            if (qty <= 0)
                return (SolidColorBrush)App.Current.Resources["Warning"];
            else
                return (SolidColorBrush)App.Current.Resources["PrimaryBlue"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}