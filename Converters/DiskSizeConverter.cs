using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class DiskSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size)
            {
                double megabytes = size / (1024.0 * 1024.0);
                double roundedMB = Math.Round(megabytes, 2);

                if (roundedMB >= 1024)
                {
                    double gb = roundedMB / 1024;
                    return $"{gb:F2} GB";
                }
                else
                {
                    return $"{roundedMB:F1} MB";
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
