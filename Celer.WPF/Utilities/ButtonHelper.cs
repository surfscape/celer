using System.Windows;
using System.Windows.Media;

namespace Celer.Utilities
{
    public static class ButtonHelper
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(ButtonHelper), new PropertyMetadata(new CornerRadius(0)));

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);
        public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);




        public static readonly DependencyProperty BackgroundHoverProperty =
            DependencyProperty.RegisterAttached("BackgroundHover", typeof(SolidColorBrush), typeof(ButtonHelper), new PropertyMetadata(new SolidColorBrush()));

        public static void SetBackgroundHover(DependencyObject obj, SolidColorBrush value) => obj.SetValue(BackgroundHoverProperty, value);
        public static SolidColorBrush GetBackgroundHover(DependencyObject obj) => (SolidColorBrush)obj.GetValue(BackgroundHoverProperty);

    }
}
