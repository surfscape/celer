using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters.Battery
{
    public class ChargingConverter : IValueConverter
    {
        public string ChargingText { get; set; } = "A carregar";
        public string DischargingText { get; set; } = "A descarregar";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isCharging && isCharging
                ? ChargingText
                : DischargingText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
