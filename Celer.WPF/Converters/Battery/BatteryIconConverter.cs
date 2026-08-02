using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters.Battery
{
    public class BatteryIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int batteryLevel)
            {
                if (batteryLevel >= 65)
                    return MahApps.Metro.IconPacks.PackIconLucideKind.BatteryFull;
                if (batteryLevel >= 45)
                    return MahApps.Metro.IconPacks.PackIconLucideKind.BatteryMedium;
                if (batteryLevel < 45)
                    return MahApps.Metro.IconPacks.PackIconLucideKind.BatteryLow;
            }
            return MahApps.Metro.IconPacks.PackIconLucideKind.BatteryWarning;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => throw new NotSupportedException();
    }
}
