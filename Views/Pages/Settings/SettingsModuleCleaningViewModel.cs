using Celer.Properties;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsModuleCleaningViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;

        [ObservableProperty]
        public partial bool EnableExportCleaningLog { get; set; } = MainConfiguration.Default.CLEANENGINE_ExportLog;

        public SettingsModuleCleaningViewModel(SettingsNavigation settingsNavigation)
        {
            _settingsNavigation = settingsNavigation;
            _settingsNavigation.PageTitle = "Cleaning options";
        }
    }
}
