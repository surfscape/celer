using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Celer.Utilities;

/// <summary>
/// Class that provides both bubble event and smooth scrolling support for WPF ScrollViewer
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

	public static readonly DependencyProperty DisableOuterScrollProperty =
		DependencyProperty.RegisterAttached("DisableOuterScroll", typeof(bool), typeof(ScrollUtilities), new PropertyMetadata(false, OnDisableOuterScrollChanged));

	public static bool GetDisableOuterScroll(DependencyObject obj) => (bool)obj.GetValue(DisableOuterScrollProperty);
	public static void SetDisableOuterScroll(DependencyObject obj, bool value) => obj.SetValue(DisableOuterScrollProperty, value);

	private static readonly DependencyProperty SuppressCountProperty =
		DependencyProperty.RegisterAttached("SuppressCount", typeof(int), typeof(ScrollUtilities), new PropertyMetadata(0));
	private static readonly DependencyProperty SuppressedViewerProperty =
		DependencyProperty.RegisterAttached("SuppressedViewer", typeof(ScrollViewer), typeof(ScrollUtilities), new PropertyMetadata(null));

	private static void OnDisableOuterScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not FrameworkElement element)
			return;

		if ((bool)e.NewValue)
		{
			element.Loaded += OnSuppressLoaded;
			element.Unloaded += OnSuppressUnloaded;
			if (element.IsLoaded)
				SuppressOuter(element);
		}
		else
		{
			element.Loaded -= OnSuppressLoaded;
			element.Unloaded -= OnSuppressUnloaded;
			RestoreOuter(element);
		}
	}

	private static void OnSuppressLoaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement element && GetDisableOuterScroll(element))
			SuppressOuter(element);
	}

	private static void OnSuppressUnloaded(object sender, RoutedEventArgs e)
	{
		if (sender is FrameworkElement element)
			RestoreOuter(element);
	}

	private static void SuppressOuter(FrameworkElement element)
	{
		if (element.GetValue(SuppressedViewerProperty) is ScrollViewer)
			return;

		var outer = FindOuterScrollViewer(element);
		if (outer == null)
		{
			element.Dispatcher.BeginInvoke(
				System.Windows.Threading.DispatcherPriority.Loaded,
				new Action(() =>
				{
					if (GetDisableOuterScroll(element) && element.IsLoaded)
						SuppressOuter(element);
				}));
			return;
		}

		int count = (int)outer.GetValue(SuppressCountProperty);
		outer.SetValue(SuppressCountProperty, count + 1);
		if (count == 0)
			outer.SetCurrentValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

		element.SetValue(SuppressedViewerProperty, outer);
	}

	private static void RestoreOuter(FrameworkElement element)
	{
		if (element.GetValue(SuppressedViewerProperty) is not ScrollViewer outer)
			return;

		element.ClearValue(SuppressedViewerProperty);

		int count = (int)outer.GetValue(SuppressCountProperty) - 1;
		outer.SetValue(SuppressCountProperty, Math.Max(0, count));
		if (count <= 0)
			outer.InvalidateProperty(ScrollViewer.VerticalScrollBarVisibilityProperty);
	}

	private static ScrollViewer? FindOuterScrollViewer(DependencyObject start)
	{
		DependencyObject? current = start;
		while (current != null)
		{
			if (current is TabControl tab)
				return tab.Template?.FindName("PART_OuterScrollViewer", tab) as ScrollViewer;

			DependencyObject? parent = null;
			try { parent = VisualTreeHelper.GetParent(current); } catch { }
			current = parent ?? LogicalTreeHelper.GetParent(current);
		}

		return null;
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