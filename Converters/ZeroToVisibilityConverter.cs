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

            bool invert = (parameter is string s && bool.TryParse(s, out bool p)) ? p : false;

            if (!invert) {
                if (zeroValue > 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                } 
            } else
            {
                if(zeroValue > 0)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
                
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
