using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Celer.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return boolean ? (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush") : (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush");

            return (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush");
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => throw new NotImplementedException();
    }
}
