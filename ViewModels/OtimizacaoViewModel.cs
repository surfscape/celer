using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class OtimizacaoViewModel : ObservableObject
    {
        private readonly Dictionary<string, UserControl> _views = new()
        {
            { "Main", new Views.UserControls.MainApp.Otimização() },
            { "MemoryManager", new Views.UserControls.MainApp.SubOtimização.MemoryManagement() },
        };

        [ObservableProperty]
        private UserControl currentView;

        public OtimizacaoViewModel()
        {
            CurrentView = _views["Main"];
            NavigationService.Register("Otimizacao", NavigateTo);
        }

        public void NavigateTo(string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = "Main";

            if (_views.TryGetValue(viewName, out var view))
            {
                CurrentView = view;
            }
        }

        [RelayCommand]
        public void BackToMain() => CurrentView = _views["Main"];
    }
}
