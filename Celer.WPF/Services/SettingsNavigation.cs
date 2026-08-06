using Celer.Views.Pages.Settings;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Services
{
    public partial class SettingsNavigation : ObservableObject
    {
        [ObservableProperty]
        public partial string PageTitle { get; set; } = "Settings";

        private SettingsBaseViewModel _currentViewModel;
        public SettingsBaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel?.Dispose();
                _currentViewModel = value;
                OnCurrentViewModelChanged();
            }
        }

        public event Action CurrentViewModelChanged;

        private void OnCurrentViewModelChanged()
        {
            CurrentViewModelChanged?.Invoke();
        }
    }
}
