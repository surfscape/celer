using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Celer.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public SolidColorBrush TrueBrush { get; set; } = new SolidColorBrush(Colors.YellowGreen);
        public SolidColorBrush FalseBrush { get; set; } = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return boolean ? TrueBrush : FalseBrush;

            return FalseBrush;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        ) => throw new NotImplementedException();
    }
}
