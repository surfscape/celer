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

            bool invert = (bool.TryParse(parameter.ToString(), out bool p));

            return (zeroValue > 0, invert) switch
            {
                (true, false) => Visibility.Visible,
                (false, true) => Visibility.Visible,
                _ => Visibility.Collapsed
            };
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
