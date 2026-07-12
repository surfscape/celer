using Celer.Properties;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Views.Pages.Settings
{
   public partial class SettingsGeneralViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;

        [ObservableProperty]
        public partial bool StartWithWindows { get; set; } = MainConfiguration.Default.AutoStartup;

        partial void OnStartWithWindowsChanged(bool value)
        {
            MainConfiguration.Default.AutoStartup = value;
            MainConfiguration.Default.Save();
        }

        [ObservableProperty]
        public partial bool CloseShouldMinimize { get; set; } = MainConfiguration.Default.CloseShouldMinimize;

        partial void OnCloseShouldMinimizeChanged(bool value)
        {
            MainConfiguration.Default.CloseShouldMinimize = value;
            MainConfiguration.Default.Save();
        }

        [ObservableProperty]
        public partial bool UpdateGatewayLaunch { get; set; } = MainConfiguration.Default.EnableAutoSurfScapeGateway;

        partial void OnUpdateGatewayLaunchChanged(bool value)
        {
            MainConfiguration.Default.EnableAutoSurfScapeGateway = value;
            MainConfiguration.Default.Save();
        }

        public SettingsGeneralViewModel(SettingsNavigation settingsNavigation)
        {
            _settingsNavigation = settingsNavigation;
            _settingsNavigation.PageTitle = "General preferences";
        }

    }
}
