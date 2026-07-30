using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Celer.Utilities;

/// <summary>
/// Class that provides both bubble event support and smooth scrolling
/// </summary>
// Most of this class was made with information from https://stackoverflow.com/questions/1033841/is-it-possible-to-implement-smooth-scroll-in-a-wpf-listview and in regards to bubble event that was taken from https://stackoverflow.com/questions/14348517/child-elements-of-scrollviewer-preventing-scrolling-with-mouse-wheel
// There was some help from an local LLM to improve the scroll detection between mouse and touchpad since I'm not that great with math and physics 
public static class ScrollUtilities
{
    public static readonly DependencyProperty SmoothScrollProperty =
        DependencyProperty.RegisterAttached("SmoothScroll", typeof(bool), typeof(ScrollUtilities), new PropertyMetadata(false, OnSmoothScrollChanged));

    public static bool GetSmoothScroll(DependencyObject obj) => (bool)obj.GetValue(SmoothScrollProperty);
    public static void SetSmoothScroll(DependencyObject obj, bool value) => obj.SetValue(SmoothScrollProperty, value);

    private static readonly ConditionalWeakTable<ScrollViewer, ScrollBehavior> _behaviors = new();

    private static void OnSmoothScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer sv)
        {
            if ((bool)e.NewValue)
                _behaviors.GetValue(sv, key => new ScrollBehavior(key)).Enable();
            else if (_behaviors.TryGetValue(sv, out var behavior))
                behavior.Disable();
        }
    }
}

internal class ScrollBehavior(ScrollViewer sv)
{
    private readonly ScrollViewer _sv = sv;
    private double _targetOffset;
    private bool _isAnimating;
    private TimeSpan _lastRenderTime;

    public void Enable()
    {
        _sv.PreviewMouseWheel += OnMouseWheel;
        _sv.Loaded += OnLoaded;
        _sv.Unloaded += OnUnloaded;
    }

    public void Disable()
    {
        _sv.PreviewMouseWheel -= OnMouseWheel;
        _sv.Loaded -= OnLoaded;
        _sv.Unloaded -= OnUnloaded;
        StopAnimation();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => _targetOffset = _sv.VerticalOffset;

    private void OnUnloaded(object sender, RoutedEventArgs e) => StopAnimation();

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled) return;

        double current = _sv.VerticalOffset;

        if ((e.Delta > 0 && _targetOffset > current) || (e.Delta < 0 && _targetOffset < current))
            _targetOffset = current;

        double scrollAmount = -e.Delta * 0.5;
        _targetOffset = Math.Clamp(_targetOffset + scrollAmount, 0, _sv.ScrollableHeight);

        if ((current <= 0 && scrollAmount < 0) || (current >= _sv.ScrollableHeight && scrollAmount > 0))
            return;

        e.Handled = true;

        if (e.Delta % 120 != 0)
        {
            _sv.ScrollToVerticalOffset(_targetOffset);
        }
        else if (!_isAnimating)
        {
            _isAnimating = true;
            _lastRenderTime = TimeSpan.Zero;
            CompositionTarget.Rendering += OnRender;
        }
    }

    private void OnRender(object? sender, EventArgs e)
    {
        if (e is not RenderingEventArgs args) return;

        double dt = _lastRenderTime == TimeSpan.Zero ? 0.016 : (args.RenderingTime - _lastRenderTime).TotalSeconds;
        _lastRenderTime = args.RenderingTime;

        double current = _sv.VerticalOffset;
        double step = (_targetOffset - current) * (1.0 - Math.Exp(-12.0 * dt));

        if (Math.Abs(_targetOffset - current) < 1.0)
        {
            _sv.ScrollToVerticalOffset(_targetOffset);
            StopAnimation();
        }
        else
        {
            _sv.ScrollToVerticalOffset(current + step);
        }
    }

    private void StopAnimation()
    {
        if (!_isAnimating) return;
        _isAnimating = false;
        CompositionTarget.Rendering -= OnRender;
    }
}