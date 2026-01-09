using System.Windows;
using System.Windows.Controls;

namespace Celer.Utilities
{
    public class CompactTabControl : TabControl
    {
        public static readonly DependencyProperty CompactModeProperty =
            DependencyProperty.Register(
                "CompactMode",
                typeof(bool),
                typeof(CompactTabControl),
                new PropertyMetadata(false, OnCompactModeChanged));

        public bool CompactMode
        {
            get => (bool)GetValue(CompactModeProperty);
            set => SetValue(CompactModeProperty, value);
        }

        private static void OnCompactModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CompactTabControl tabControl)
            {
                bool isCompact = (bool)e.NewValue;
                foreach (var item in tabControl.Items.OfType<TabItem>())
                {
                    if (isCompact)
                    {
                        item.MinWidth = 40;
                        item.Width = 40;
                        item.Padding = new Thickness(13, 12, 13, 12);
                    }
                    else
                    {
                        item.MinWidth = 180;
                        item.Padding = new Thickness(13, 9, 16, 9);
                    }
                }
            }
        }
    }


}
