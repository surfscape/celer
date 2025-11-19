using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class PositiveToNegative : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float nv)
            {
                return (nv * 2) - 100;
            }

            return value;
        }
        public object ConvertBack(
    object value,
    Type targetType,
    object parameter,
    CultureInfo culture
) => throw new NotImplementedException();

    }
}
