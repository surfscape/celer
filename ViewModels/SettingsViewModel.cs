using Celer.Services;
using Celer.Views.Pages.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace Celer.ViewModels
{
    public partial class SettingsViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;
        private readonly SettingsShellViewModel _settingsShellViewModel;

        public SettingsBaseViewModel CurrentViewModel => _settingsNavigation.CurrentViewModel;
        public string CurrentViewTitle => _settingsNavigation.PageTitle;
        public Action? CloseWindowAction { get; set; }

        public SettingsViewModel(SettingsNavigation settingsNavigation, SettingsShellViewModel settingsShellViewModel)
        {
            _settingsNavigation = settingsNavigation;
            _settingsShellViewModel = settingsShellViewModel;
            _settingsNavigation.CurrentViewModelChanged += OnCurrentViewModelChanged;
            _settingsNavigation.CurrentViewModel = settingsShellViewModel;
        }


        [RelayCommand]
        private void GoBack()
        {
            if (_settingsNavigation.CurrentViewModel == _settingsShellViewModel) {
                CloseWindowAction?.Invoke();
            } else { 
                _settingsNavigation.CurrentViewModel = _settingsShellViewModel;
                _settingsNavigation.PageTitle = "Settings";
                OnCurrentViewModelChanged();
            }
        }
        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(CurrentViewTitle));
        }
    }
}
