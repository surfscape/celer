using Celer.Views.Pages.Settings;

namespace Celer.Services
{
    public class SettingsNavigation
    {
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
