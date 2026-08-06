using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class BoolToGpuTypeConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => (bool)value ? "Integrated" : "External";

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => throw new NotImplementedException();
    }
}
