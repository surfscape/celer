using Celer.Models;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class ManutencaoViewModel : ObservableObject
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        [ObservableProperty]
        private NavigationSubView? currentNamedView;

        public UserControl? CurrentView => CurrentNamedView?.Control;

        public string? CurrentViewName => CurrentNamedView?.Name;

        private readonly NavigationService _navigationService;

        public ManutencaoViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.Register("Manutencao", NavigateTo);

            _views = new Dictionary<string, NavigationSubView>
        {
        };

            CurrentNamedView = null;
        }

        public void NavigateTo(string viewKey)
        {
            if (string.IsNullOrEmpty(viewKey) || viewKey == "Main")
            {
                CurrentNamedView = null;
                return;
            }

            if (_views.TryGetValue(viewKey, out var view))
            {
                CurrentNamedView = view;
            }
        }

        [RelayCommand]
        public void Navigate(string viewKey) => NavigateTo(viewKey);

        [RelayCommand]
        public void BackToMain() => NavigateTo("Main");
    }
}
