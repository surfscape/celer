using Celer.Services;
using Celer.Utilities;
using Celer.ViewModels;
using Celer.Views.Pages.Settings;

namespace Celer.Views.Windows
{
	/// <summary>
	/// Interaction logic for Settings.xaml
	/// </summary>
	public partial class Settings : BaseWindow
	{
		private readonly SettingsViewModel _viewModel;
		private readonly SettingsNavigation _settingsNavigation;
		private readonly SettingsShellViewModel _settingsShellViewModel;

		public Settings(SettingsViewModel viewModel, SettingsNavigation settingsNavigation, SettingsShellViewModel settingsShellView)
		{
			_viewModel = viewModel;
			_settingsNavigation = settingsNavigation;
			_settingsShellViewModel = settingsShellView;
			DataContext = _viewModel;
			InitializeComponent();
			viewModel.CloseWindowAction = Close;
		}
	}
}
