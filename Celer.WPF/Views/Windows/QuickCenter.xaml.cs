using Celer.Utilities;
using Celer.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace Celer.Views.Windows
{
	/// <summary>
	/// Interaction logic for QuickCenter.xaml
	/// </summary>
	// TODO: QuickCenter Window code is pretty ugly and was cobbled up together from various StackOverflow answers but I have the intention to cleaned it up
	public partial class QuickCenter : BaseWindow, IDisposable
	{
		private readonly QuickCenterViewModel _viewModel;
		private readonly EventHandler? _deactivatedHandler;

		public QuickCenter(QuickCenterViewModel viewModel)
		{
			_viewModel = viewModel;
			InitializeComponent();
			DataContext = _viewModel;

			_deactivatedHandler = (s, e) => Close();

			Deactivated += _deactivatedHandler;

		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			Dispose();
		}

		public void Dispose()
		{
			Deactivated -= _deactivatedHandler;
			GC.SuppressFinalize(this);
		}

		const int WM_SYSCOMMAND = 0x0112;
		const int SC_MOVE = 0xF010;

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{

			switch (msg)
			{
				case WM_SYSCOMMAND:
					int command = wParam.ToInt32() & 0xfff0;
					if (command == SC_MOVE)
					{
						handled = true;
					}
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			WindowInteropHelper helper = new(this);
			HwndSource source = HwndSource.FromHwnd(helper.Handle);
			source.AddHook(WndProc);
			PositionWindowAtBottomRight();
		}
		private void PositionWindowAtBottomRight()
		{
			var desktopWorkingArea = SystemParameters.WorkArea;
			Left = desktopWorkingArea.Right - Width - 12;
			Top = desktopWorkingArea.Bottom - Height - 12;
		}
	}
}
