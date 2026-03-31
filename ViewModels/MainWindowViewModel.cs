using Celer.Models;
using Celer.Services;
using Celer.Views.UserControls.MainApp;
using Celer.Views.UserControls.MainWindow;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {

        [ObservableProperty]
        private int selectedTabIndex = 0;

        [ObservableProperty]
        private bool tabControlCompactMode;

        [ObservableProperty]
        private bool isCompact = false;

        [ObservableProperty]
        private bool canGoBack;

        [ObservableProperty]
        private ObservableCollection<TabModule> tabsModule;

        [ObservableProperty]
        private UserControl menuBarControl;

        private readonly NavigationService _navigationService;
        private readonly IServiceProvider _serviceProvider;

        public MainWindowViewModel(
            NavigationService navigationService,
            IServiceProvider serviceProvider
        )
        {
            _navigationService = navigationService;
            _navigationService.NavigateTo = NavigateTo;
            _serviceProvider = serviceProvider;
            TabControlCompactMode = _navigationService.CompactMode;
            _navigationService.CompactModeChanged += OnCompactModeChanged;
            _navigationService.NavigationChanged += OnNavigationChanged;
            MenuBarControl = _serviceProvider.GetRequiredService<MenuBar>();
            TabsModule =
        [
            new() { Title = "Dashboard", Icon = PackIconLucideKind.HeartPulse, Content = _serviceProvider.GetRequiredService<Dashboard>() },
            new() { Title = "Cleaning", Icon = PackIconLucideKind.Trash, Content = _serviceProvider.GetRequiredService<Limpeza>(), VerticalScrollMode = ScrollBarVisibility.Disabled },
            new() { Title = "Optimization", Icon = PackIconLucideKind.Rocket, Content = _serviceProvider.GetRequiredService<Optimization>() },
            new() { Title = "Maintenance", Icon = PackIconLucideKind.Construction, Content = _serviceProvider.GetRequiredService<Maintenance>() },
            new() { Title = "Privacy & Security", Icon = PackIconLucideKind.Shield, Content = _serviceProvider.GetRequiredService<Privacidade>() }
        ];
        }

        private void OnCompactModeChanged(object sender, bool isCompact)
        {
            TabControlCompactMode = isCompact;
        }

        private void NavigateTo(string tabName, string subview)
        {
            var tab = TabsModule.FirstOrDefault(tbId => tbId.Title == tabName);
            if (tab != null)
            {
                SelectedTabIndex = TabsModule.IndexOf(tab);
                _navigationService.NavigateInternal(tabName, subview);
            }
        }
        partial void OnSelectedTabIndexChanged(int value)
        {
            var tabName = TabsModule[value].Title != null ? TabsModule[value].Title : TabsModule[0].Title;
            if (string.IsNullOrEmpty(tabName))
                return;

            var innerView = _navigationService.GetInnerViewForTab(tabName);
            _navigationService.NavigateInternal(tabName, innerView);
        }

        [RelayCommand]
        private void NavigateToTab(string tab)
        {
            _navigationService.Navigate(tab);
        }

        [RelayCommand]
        private void ToggleCompactMode()
        {
            _navigationService.CompactMode = !_navigationService.CompactMode;
            IsCompact = _navigationService.CompactMode;
        }


        [RelayCommand]
        private void GoBack()
        {
            _navigationService.BackToParent();
        }

        private void OnNavigationChanged(string? tab, string? innerView)
        {
            CanGoBack = !string.IsNullOrEmpty(innerView) && !string.Equals(innerView, "Main", StringComparison.Ordinal);
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
