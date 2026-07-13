using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Celer.Utilities
{
    /// <summary>
    /// Class that provides both bubble event support and also smooth scrolling
    /// </summary>
    // Most of this class was made with information from https://stackoverflow.com/questions/1033841/is-it-possible-to-implement-smooth-scroll-in-a-wpf-listview and in regards to bubble event that was taken from https://stackoverflow.com/questions/14348517/child-elements-of-scrollviewer-preventing-scrolling-with-mouse-wheel
    // There was some help from an local LLM to improve the scroll detection between mouse and touchpad since I'm not that great with math and physics
    public static class ScrollUtilities
    {
        public static readonly DependencyProperty SmoothScrollProperty =
            DependencyProperty.RegisterAttached(
                "SmoothScroll",
                typeof(bool),
                typeof(ScrollUtilities),
                new PropertyMetadata(false, OnSmoothScrollChanged));

        public static bool GetSmoothScroll(DependencyObject obj) => (bool)obj.GetValue(SmoothScrollProperty);
        public static void SetSmoothScroll(DependencyObject obj, bool value) => obj.SetValue(SmoothScrollProperty, value);

        private static readonly DependencyProperty CurrentVerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "CurrentVerticalOffset",
                typeof(double),
                typeof(ScrollUtilities),
                new PropertyMetadata(0.0, OnCurrentVerticalOffsetChanged));

        private static void OnCurrentVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        private static readonly DependencyProperty TargetVerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "TargetVerticalOffset",
                typeof(double),
                typeof(ScrollUtilities),
                new PropertyMetadata(0.0));



        private static void OnSmoothScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                    scrollViewer.Loaded += (s, args) =>
                    {
                        scrollViewer.SetValue(CurrentVerticalOffsetProperty, scrollViewer.VerticalOffset);
                        scrollViewer.SetValue(TargetVerticalOffsetProperty, scrollViewer.VerticalOffset);
                    };
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                }
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer || e.Handled)
                return;


            bool isTouchpad = (e.Delta % 120 != 0);


            double targetOffset = (double)scrollViewer.GetValue(TargetVerticalOffsetProperty);
            double currentOffset = scrollViewer.VerticalOffset;

            if (Math.Abs(targetOffset - currentOffset) < 1 || (e.Delta > 0 && targetOffset > currentOffset) || (e.Delta < 0 && targetOffset < currentOffset))
            {
                targetOffset = currentOffset;
            }

            double scrollAmount = -e.Delta;
            double newTarget = targetOffset + scrollAmount;
            newTarget = Math.Clamp(newTarget, 0, scrollViewer.ScrollableHeight);

            bool atTop = currentOffset <= 0 && scrollAmount < 0;
            bool atBottom = currentOffset >= scrollViewer.ScrollableHeight && scrollAmount > 0;

            if (atTop || atBottom)
            {
                return;
            }

            e.Handled = true;
            scrollViewer.SetValue(TargetVerticalOffsetProperty, newTarget);

            if (isTouchpad)
            {
                scrollViewer.BeginAnimation(CurrentVerticalOffsetProperty, null);
                scrollViewer.SetValue(CurrentVerticalOffsetProperty, newTarget);
            }
            else
            {
                var animation = new DoubleAnimation
                {
                    To = newTarget,
                    Duration = TimeSpan.FromMilliseconds(120),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                    FillBehavior = FillBehavior.HoldEnd
                };

                scrollViewer.BeginAnimation(CurrentVerticalOffsetProperty, animation, HandoffBehavior.SnapshotAndReplace);
            }
        }
    }
}
