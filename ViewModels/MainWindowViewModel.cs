using Celer.Services;
using Celer.Views.UserControls.MainApp;
using Celer.Views.UserControls.MainWindow;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty] private UserControl menuBarControl;
        [ObservableProperty] private UserControl dashboardControl;
        [ObservableProperty] private UserControl otimizacaoControl;
        [ObservableProperty] private UserControl manutencaoControl;


        private readonly Dictionary<string, int> _tabIndexes = new()
        {
            { "Dashboard", 0 },
            { "Limpeza", 1 },
            { "Otimizacao", 2 },
            { "Manutencao", 3 },
            { "Privacidade", 4 },
            { "Avancado", 5 }
        };

        private readonly NavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(NavigationService navigationService, IServiceProvider serviceProvider)
        {

            _navigationService = navigationService;
            _navigationService.NavigateTo = NavigateTo;
            _serviceProvider = serviceProvider;
            MenuBarControl = _serviceProvider.GetRequiredService<MenuBar>();
            DashboardControl = _serviceProvider.GetRequiredService<Dashboard>();
            OtimizacaoControl = _serviceProvider.GetRequiredService<Otimizacao>();
            ManutencaoControl = _serviceProvider.GetRequiredService<Manutencao>();
        }

        private void NavigateTo(string tabName, string subview)
        {
            if (_tabIndexes.TryGetValue(tabName, out var index))
            {
                SelectedTabIndex = index;
                _navigationService.NavigateInternal(tabName, subview);
            }
        }
    }
}
