using Celer.Properties;
using Celer.Utilities;
using Celer.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using static Celer.ViewModels.MainWindowViewModel;

namespace Celer.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : BaseWindow
{
	private readonly MainWindowViewModel _viewModel;

	public MainWindow(MainWindowViewModel viewModel)
	{
		_viewModel = viewModel;
		IsVisibleChanged += new DependencyPropertyChangedEventHandler(CheckVisibility);
		InitializeComponent();
		DataContext = _viewModel;
		WeakReferenceMessenger.Default.Register<WindowVisibleMessage>(this, (r, m) =>
		{
			if (m.Value)
			{
				ToggleMainWindowMenu.Header = "Hide Celer";
			}
			else
				ToggleMainWindowMenu.Header = "Show Celer";
		});
		Activated += (_, _) => OnActivated();
		Deactivated += (_, _) => OnDeactivated();
	}

	// This activates what is known as EcoQoS which lowers the process priority to reduce resources and power consumption.
	// It's triggered when the MainWindow is unfocused (minimized, closed to the system tray, or when the user takes focus out of the window).
	// However it doesn't check for any running task happening (ex: Celer currently cleaning something in the background), so if the user runs a task and minimizes the app, the performance will be heavily hit.
	// This is planned in the feature where it will be possible to get the current task Celer is doing.
	private static void OnActivated()
	{
		ProcessPowerManager.Disable();
	}

	private static void OnDeactivated()
	{
		ProcessPowerManager.Enable();
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
		source?.AddHook(WndProc);
	}

	private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == App.WH_SHOWME)
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();
			Topmost = true;
			Topmost = false;
			Focus();
			WeakReferenceMessenger.Default.Send(new WindowVisibleMessage(true));
		}
		return IntPtr.Zero;
	}

	void CheckVisibility(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (Visibility == Visibility.Visible && DataContext == null)
		{
			DataContext = _viewModel;
			ProcessPowerManager.Disable();
		}
	}


	protected override void OnClosing(CancelEventArgs e)
	{
		if (MainConfiguration.Default.CloseShouldMinimize)
		{
			e.Cancel = true;
			Visibility = Visibility.Collapsed;
			ProcessPowerManager.Enable();
			WeakReferenceMessenger.Default.Send(new WindowVisibleMessage(false));
		}
		else
			e.Cancel = false;
	}
}



