using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class ManutencaoViewModel : ObservableObject
    {
        private readonly Dictionary<string, UserControl> _views = new()
        {
            { "BatteryManagement", new Views.UserControls.MainApp.SubManutencao.Battery() },
        };

        [ObservableProperty]
        private UserControl currentView;

        public ManutencaoViewModel()
        {
            CurrentView = null;
            NavigationService.Register("Manutencao", NavigateTo);
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
    }
}
