using Celer.Utilities;
using Celer.ViewModels;
using System.ComponentModel;

namespace Celer.Views.Windows
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : BaseWindow
	{
		private readonly SettingsViewModel _viewModel;

		public Settings(SettingsViewModel viewModel)
		{
			_viewModel = viewModel;
			DataContext = _viewModel;
			InitializeComponent();
			viewModel.CloseWindowAction = Close;
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			_viewModel.GoToShell();
		}
	}
}
