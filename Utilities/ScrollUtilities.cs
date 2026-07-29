using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Celer.Utilities
{
    /// <summary>
    /// Class that provides both bubble event support and smooth scrolling
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

        private static readonly DependencyProperty TargetVerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "TargetVerticalOffset",
                typeof(double),
                typeof(ScrollUtilities),
                new PropertyMetadata(0.0));

        private static readonly DependencyProperty IsAnimatingProperty =
            DependencyProperty.RegisterAttached(
                "IsAnimating",
                typeof(bool),
                typeof(ScrollUtilities),
                new PropertyMetadata(false));

        private static readonly DependencyProperty RenderingHandlerProperty =
            DependencyProperty.RegisterAttached(
                "RenderingHandler",
                typeof(EventHandler),
                typeof(ScrollUtilities));

        private static void OnSmoothScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                    scrollViewer.Loaded += ScrollViewer_Loaded;
                    scrollViewer.Unloaded += ScrollViewer_Unloaded;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                    scrollViewer.Loaded -= ScrollViewer_Loaded;
                    scrollViewer.Unloaded -= ScrollViewer_Unloaded;
                    StopAnimating(scrollViewer);
                }
            }
        }

        private static void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                scrollViewer.SetValue(TargetVerticalOffsetProperty, scrollViewer.VerticalOffset);
            }
        }

        private static void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                StopAnimating(scrollViewer);
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer || e.Handled)
                return;

            bool isTouchpad = (e.Delta % 120 != 0);

            double currentOffset = scrollViewer.VerticalOffset;
            double targetOffset = (double)scrollViewer.GetValue(TargetVerticalOffsetProperty);

            if ((e.Delta > 0 && targetOffset > currentOffset) || (e.Delta < 0 && targetOffset < currentOffset))
            {
                targetOffset = currentOffset;
            }

            double multiplier = 0.5;
            double scrollAmount = -e.Delta * multiplier;

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
                scrollViewer.ScrollToVerticalOffset(newTarget);
            }
            else
            {
                if (!(bool)scrollViewer.GetValue(IsAnimatingProperty))
                {
                    scrollViewer.SetValue(IsAnimatingProperty, true);
                    CompositionTarget.Rendering += GetRenderingHandler(scrollViewer);
                }
            }
        }

        private static EventHandler GetRenderingHandler(ScrollViewer scrollViewer)
        {
            if (scrollViewer.GetValue(RenderingHandlerProperty) is not EventHandler handler)
            {
                TimeSpan lastRenderTime = TimeSpan.Zero;

                handler = (sender, args) =>
                {
                    var renderingArgs = (RenderingEventArgs)args;

                    double deltaTime = (renderingArgs.RenderingTime - lastRenderTime).TotalSeconds;
                    if (lastRenderTime == TimeSpan.Zero) deltaTime = 0.016;
                    lastRenderTime = renderingArgs.RenderingTime;

                    double current = scrollViewer.VerticalOffset;
                    double target = (double)scrollViewer.GetValue(TargetVerticalOffsetProperty);

                    double speed = 12.0;
                    double factor = 1.0 - Math.Exp(-speed * deltaTime);
                    double step = (target - current) * factor;

                    if (Math.Abs(target - current) < 1.0)
                    {
                        scrollViewer.ScrollToVerticalOffset(target);
                        StopAnimating(scrollViewer);
                        lastRenderTime = TimeSpan.Zero;
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(current + step);
                    }
                };
                scrollViewer.SetValue(RenderingHandlerProperty, handler);
            }
            return handler;
        }

        private static void StopAnimating(ScrollViewer scrollViewer)
        {
            if ((bool)scrollViewer.GetValue(IsAnimatingProperty))
            {
                scrollViewer.SetValue(IsAnimatingProperty, false);
                if (scrollViewer.GetValue(RenderingHandlerProperty) is EventHandler handler)
                {
                    CompositionTarget.Rendering -= handler;
                }
            }
        }
    }
}
