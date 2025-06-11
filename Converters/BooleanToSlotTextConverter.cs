﻿using System.Globalization;
using System.Windows.Data;

namespace Celer.Converters
{
    public class BooleanToSlotTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "Slot Ocupado" : "Slot Vazio";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
