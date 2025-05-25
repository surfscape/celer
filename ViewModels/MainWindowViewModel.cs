using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndex;

        private readonly Dictionary<string, int> _tabIndexes = new()
        {
            { "Dashboard", 0 },
            { "Limpeza", 1 },
            { "Otimizacao", 2 },
            { "Manutencao", 3 },
            { "Privacidade", 4 },
            { "Avancado", 5 }
        };

        public MainWindowViewModel()
        {
            NavigationService.NavigateTo = NavigateTo;
        }

        private void NavigateTo(string tabName, string subview)
        {
            if (_tabIndexes.TryGetValue(tabName, out var index))
            {
                SelectedTabIndex = index;
                NavigationService.NavigateInternal(tabName, subview);
            }
        }
    }
}
