using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Celer.Utilities
{
	[TemplatePart(Name = PartMainGrid, Type = typeof(Grid))]
	[TemplatePart(Name = PartHighContrastBorder, Type = typeof(Border))]
	[TemplatePart(Name = PartMinButton, Type = typeof(Button))]
	[TemplatePart(Name = PartMaxButton, Type = typeof(Button))]
	[TemplatePart(Name = PartCloseButton, Type = typeof(Button))]
	[TemplatePart(Name = PartTitle, Type = typeof(TextBlock))]
	public partial class BaseWindow : Window
	{
		[DependencyProperty]
		public bool IsWindowControlsVisible { get; set; } = true;

		[DependencyProperty]
		public object? NavigationControls { get; set; }

		[DependencyProperty]
		public object? TitleBarMenu { get; set; }

		[DependencyProperty]
		public object? WindowTitle { get; set; }

		void OnIsWindowControlsVisibleChanged()
		{
			ApplyCaptionStyle();
		}

		void OnWindowTitleChanged(object value)
		{
			if (value is not null)
				_title?.Visibility = Visibility.Collapsed;
		}

		private const string PartMainGrid = "PART_MainGrid";
		private const string PartHighContrastBorder = "PART_HighContrastBorder";
		private const string PartMinButton = "PART_MinButton";
		private const string PartMaxButton = "PART_MaxButton";
		private const string PartCloseButton = "PART_CloseButton";
		private const string PartTitle = "PART_Title";

		private Grid? _mainGrid;
		private Border? _highContrastBorder;
		private Button? _minButton;
		private Button? _maxButton;
		private Button? _closeButton;
		private TextBlock? _title;

		private static bool SystemDrawsCaption => !IsBackdropDisabled() && IsBackdropSupported() && !SystemParameters.HighContrast;

		static BaseWindow()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(BaseWindow),
				new FrameworkPropertyMetadata(typeof(BaseWindow)));
		}
		public BaseWindow()
		{
			StateChanged += (_, _) => UpdateWindowVisuals();
			Activated += (_, _) => UpdateWindowVisuals();
			Deactivated += (_, _) => UpdateWindowVisuals();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			WindowChrome.SetWindowChrome(this, new WindowChrome
			{
				CaptionHeight = 44,
				CornerRadius = new CornerRadius(12),
				GlassFrameThickness = new Thickness(-1),
				ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
				UseAeroCaptionButtons = SystemDrawsCaption,
				NonClientFrameEdges = GetPrefferedNonClientFrameEdges()
			});
			UpdateWindowVisuals();
			ApplyCaptionStyle();
		}

		private void ApplyCaptionStyle()
		{
			if (IsWindowControlsVisible) return;

			var hwnd = new HWND(new WindowInteropHelper(this).Handle);
			if (hwnd.IsNull) return;

			int style = PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
			_ = PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE,
								  style & ~(int)WINDOW_STYLE.WS_SYSMENU);

			PInvoke.SetWindowPos(hwnd, HWND.Null, 0, 0, 0, 0,
				SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
				SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED);
		}

		private void UpdateTitleBarButtonsVisibility()
		{
			if (_minButton != null && _maxButton != null && _closeButton != null)
			{
				if (IsBackdropDisabled() || !IsBackdropSupported() ||
						SystemParameters.HighContrast == true)
				{
					_minButton.Visibility = Visibility.Visible;
					_maxButton.Visibility = Visibility.Visible;
					_closeButton.Visibility = Visibility.Visible;
				}
				else
				{
					_minButton.Visibility = Visibility.Collapsed;
					_maxButton.Visibility = Visibility.Collapsed;
					_closeButton.Visibility = Visibility.Collapsed;
				}
			}
		}

		private void ToggleMaximize() =>
	WindowState = WindowState == WindowState.Maximized
		? WindowState.Normal
		: WindowState.Maximized;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_mainGrid = GetTemplateChild(PartMainGrid) as Grid;
			_highContrastBorder = GetTemplateChild(PartHighContrastBorder) as Border;
			_minButton = GetTemplateChild(PartMinButton) as Button;
			_maxButton = GetTemplateChild(PartMaxButton) as Button;
			_closeButton = GetTemplateChild(PartCloseButton) as Button;
			_title = GetTemplateChild(PartTitle) as TextBlock;

			_minButton?.Click += (_, _) => SystemCommands.MinimizeWindow(this);
			_maxButton?.Click += (_, _) => ToggleMaximize();
			_closeButton?.Click += (_, _) => Close();
			_title?.Text = Title;
			_title?.Visibility = WindowTitle is not null ? Visibility.Collapsed : Visibility.Visible;
			UpdateWindowVisuals();
		}

		protected virtual void UpdateWindowVisuals()
		{
			if (_mainGrid is null || _highContrastBorder is null) return;
			_mainGrid.Margin = default;
			if (WindowState == WindowState.Maximized)
			{
				_mainGrid.Margin = SystemParameters.HighContrast ? new Thickness(0, 8, 0, 0) : new Thickness(4, 8, 4, 4);
			}
			UpdateTitleBarButtonsVisibility();
			if (SystemParameters.HighContrast == true)
			{
				_highContrastBorder.SetResourceReference(BorderBrushProperty, IsActive ? SystemColors.ActiveCaptionBrushKey :
																						SystemColors.InactiveCaptionBrushKey);
				_highContrastBorder.BorderThickness = new Thickness(8, 1, 8, 8);
			}
			else
			{
				_highContrastBorder.BorderBrush = Brushes.Transparent;
				_highContrastBorder.BorderThickness = new Thickness(0);
				_highContrastBorder.Margin = new Thickness(0, 0, 0, -4);
			}

			if (IsWindows11OrGreater())
			{
				WindowChrome wc = WindowChrome.GetWindowChrome(this);
				wc?.NonClientFrameEdges = GetPrefferedNonClientFrameEdges();
			}
		}

		private static NonClientFrameEdges GetPrefferedNonClientFrameEdges()
		{
			if (SystemParameters.HighContrast == true || IsWindows11OrGreater() == false)
				return NonClientFrameEdges.None;

			return NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left;
		}

		public static bool IsBackdropDisabled()
		{
			var appContextBackdropData = AppContext.GetData("Switch.System.Windows.Appearance.DisableFluentThemeWindowBackdrop");
			bool disableFluentThemeWindowBackdrop = false;

			if (appContextBackdropData != null)
				disableFluentThemeWindowBackdrop = bool.Parse((string)appContextBackdropData);

			return disableFluentThemeWindowBackdrop;
		}
		public static bool IsBackdropSupported()
		{
			var os = Environment.OSVersion;
			var version = os.Version;

			return version.Major >= 10 && version.Build >= 22621;
		}

		public static bool IsWindows11OrGreater()
		{
			var os = Environment.OSVersion;
			var version = os.Version;

			return (version.Major >= 10 && version.Build >= 22000);
		}
	}
}
