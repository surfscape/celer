using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class NullableBoolToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as bool? == true;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            return Binding.DoNothing;
        }
    }
}
