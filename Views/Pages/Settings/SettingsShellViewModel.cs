using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsShellViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string name = "Hello World";

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
        private void NavigateToAdvanced()
        {
            _settingsNavigation.CurrentViewModel = _serviceProvider.GetRequiredService<SettingsAdvancedViewModel>();
        }
    }
}
