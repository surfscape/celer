using Celer.Services;
using Celer.Views.UserControls.MainApp.SubOtimização;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class OtimizacaoViewModel : ObservableObject
    {
        private readonly Dictionary<string, UserControl> _views;

        [ObservableProperty]
        private UserControl? currentView;

        private readonly NavigationService _navigationService;

        public OtimizacaoViewModel(NavigationService navigationService, MemoryManagement memoryView)
        {
            _navigationService = navigationService;
            _navigationService.Register("Otimizacao", NavigateTo);

            _views = new Dictionary<string, UserControl>
            {
                { "Memory", memoryView }
            };

            CurrentView = null;
        }

        public void NavigateTo(string viewName)
        {
            if (string.IsNullOrEmpty(viewName) || viewName == "Main")
            {
                CurrentView = null;
                return;
            }

            if (_views.TryGetValue(viewName, out var view))
            {
                CurrentView = view;
            }
        }

        [RelayCommand]
        public void Navigate(string viewName) => NavigateTo(viewName);

        [RelayCommand]
        public void BackToMain() => NavigateTo("Main");
    }
}
