using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Celer.Converters
{
    internal class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int zeroValue = (value is int b) ? b : 0;

            bool invert = (parameter is string s && bool.TryParse(s, out bool p)) && p;

            if (!invert)
                return zeroValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            else
                return zeroValue > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }
}
