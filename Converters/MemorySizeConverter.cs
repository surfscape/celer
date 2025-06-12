using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class MemorySizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double mb)
            {
                if (mb >= 1024)
                {
                    double gb = mb / 1024;
                    return $"{gb:F2} GB";
                }
                else
                {
                    return $"{mb:F0} MB";
                }
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
