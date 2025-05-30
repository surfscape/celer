using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Celer.Views.UserControls.MainApp.SubManutencao;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using Celer.Models;

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

        public ManutencaoViewModel(NavigationService navigationService, Battery batteryView)
        {
            _navigationService = navigationService;
            _navigationService.Register("Manutencao", NavigateTo);

            _views = new Dictionary<string, NavigationSubView>
        {
            { "Battery", new NavigationSubView("Estado da Bateria", batteryView) },
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
