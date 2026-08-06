using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class MenuStateToolTipConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => (bool)value ? "Open Navigation" : "Close Navigation";

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
