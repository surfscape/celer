using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Celer.Views.UserControls.MainApp.SubManutencao;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class ManutencaoViewModel : ObservableObject
    {
        private readonly Dictionary<string, UserControl> _views;

        [ObservableProperty]
        private UserControl? currentView;


        private readonly NavigationService _navigationService;
        public ManutencaoViewModel(NavigationService navigationService, Battery batterView)
        {
            _navigationService = navigationService;
            _navigationService.Register("Manutencao", NavigateTo);

            _views = new Dictionary<string, UserControl>
            {
                { "Battery", batterView },
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
