using Celer.Services;
using Celer.Views.UserControls.MainApp;
using Celer.Views.UserControls.MainWindow;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private bool tabControlCompactMode;

        private readonly Lazy<UserControl> _menuBarControl;
        private readonly Lazy<UserControl> _dashboardControl;
        private readonly Lazy<UserControl> _limpezaControl;
        private readonly Lazy<UserControl> _optimizationControl;
        private readonly Lazy<UserControl> _maintenanceControl;

        public UserControl MenuBarControl => _menuBarControl.Value;
        public UserControl DashboardControl => _dashboardControl.Value;
        public UserControl LimpezaControl => _limpezaControl.Value;
        public UserControl OptimizationControl => _optimizationControl.Value;
        public UserControl MaintenanceControl => _maintenanceControl.Value;

        private readonly Dictionary<string, int> _tabIndexes = new()
        {
            { "Dashboard", 0 },
            { "Cleaning", 1 },
            { "Optimization", 2 },
            { "Maintenance", 3 },
            { "Opsec", 4 },
        };

        private readonly NavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(
            NavigationService navigationService,
            IServiceProvider serviceProvider
        )
        {
            _navigationService = navigationService;
            _navigationService.NavigateTo = NavigateTo;
            _navigationService.CompactModeChanged += OnCompactModeChanged;
            _serviceProvider = serviceProvider;
            _menuBarControl = new Lazy<UserControl>(() => _serviceProvider.GetRequiredService<MenuBar>());
            _dashboardControl = new Lazy<UserControl>(() => _serviceProvider.GetRequiredService<Dashboard>());
            _limpezaControl = new Lazy<UserControl>(() => _serviceProvider.GetRequiredService<Limpeza>());
            _optimizationControl = new Lazy<UserControl>(() => _serviceProvider.GetRequiredService<Optimization>());
            _maintenanceControl = new Lazy<UserControl>(() => _serviceProvider.GetRequiredService<Maintenance>());
            TabControlCompactMode = _navigationService.CompactMode;
        }

        private void OnCompactModeChanged(object sender, bool isCompact)
        {
            TabControlCompactMode = isCompact;
        }

        private void NavigateTo(string tabName, string subview)
        {
            if (_tabIndexes.TryGetValue(tabName, out var index))
            {
                SelectedTabIndex = index;
                _navigationService.NavigateInternal(tabName, subview);
            }
        }
        partial void OnSelectedTabIndexChanged(int value)
        {
            var tabName = _tabIndexes.FirstOrDefault(kv => kv.Value == value).Key;
            if (string.IsNullOrEmpty(tabName))
                return;

            var innerView = _navigationService.GetInnerViewForTab(tabName);
            _navigationService.NavigateInternal(tabName, innerView);
        }
    }
    public partial class QCMenuViewModel : ObservableObject
    {
        [RelayCommand]
        private static void QCExitApp()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private static void QCOpenApp()
        {
            if (Application.Current.MainWindow.DataContext == null)
            {
                Application.Current.MainWindow.DataContext = App.AppHost.Services.GetService<MainWindowViewModel>();
            }
            Application.Current.MainWindow.Visibility = Visibility.Visible;
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.Activate();
            Application.Current.MainWindow.Focus();
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }
    }
}
