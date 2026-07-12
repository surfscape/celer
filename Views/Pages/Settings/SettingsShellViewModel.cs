using Celer.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsShellViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;
        private readonly IServiceProvider _serviceProvider;

        public SettingsShellViewModel(SettingsNavigation settingsNavigation, IServiceProvider serviceProvider)
        {
            _settingsNavigation = settingsNavigation;
            _serviceProvider = serviceProvider;
        }

        [RelayCommand]
        private void NavigateToGeneral()
        {
            _settingsNavigation.CurrentViewModel = _serviceProvider.GetRequiredService<SettingsGeneralViewModel>();
        }

        [RelayCommand]
        private void NavigateToInterface()
        {
            _settingsNavigation.CurrentViewModel = _serviceProvider.GetRequiredService<SettingsInterfaceViewModel>();
        }

        [RelayCommand]
        private void NavigateToAdvanced()
        {
            _settingsNavigation.CurrentViewModel = _serviceProvider.GetRequiredService<SettingsAdvancedViewModel>();
        }
    }
}
