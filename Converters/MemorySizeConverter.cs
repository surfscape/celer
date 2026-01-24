using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class MemorySizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool rounding = false;
            if (parameter == null)
                parameter = false;

            if (value is double mb)
            {
                if (mb >= 1024 && bool.TryParse(parameter.ToString(), out rounding))
                {
                    double gb = mb / 1024;
                    return rounding ? $"{gb:F0} GB" : $"{gb:F2} GB";
                }
                else
                {
                    return rounding ? $"{mb:F0} GB" : $"{mb:F2} GB";
                }
          ;
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
