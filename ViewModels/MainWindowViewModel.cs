using Celer.Models;
using Celer.Properties;
using Celer.Services;
using Celer.Views.UserControls.MainApp;
using Celer.Views.UserControls.MainWindow;
using Celer.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using static Celer.Views.Pages.Settings.SettingsAdvancedViewModel;

namespace Celer.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public QCMenuViewModel MenuViewModel { get; } = new QCMenuViewModel();

        [ObservableProperty]
        private int selectedTabIndex = 0;

        [ObservableProperty]
        private bool tabControlCompactMode;

        [ObservableProperty]
        private bool isCompact = false;

        [ObservableProperty]
        private bool canGoBack;

        [ObservableProperty]
        public partial ObservableCollection<TabModule> TabsModule { get; set; }

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
            new() { Title = "Dashboard", Icon = PackIconLucideKind.SquareActivity, Content = _serviceProvider.GetRequiredService<Dashboard>(), VerticalScrollMode = ScrollBarVisibility.Disabled  },
            new() { Title = "Cleaning", Icon = PackIconLucideKind.Trash, Content = _serviceProvider.GetRequiredService<Cleaning>(), VerticalScrollMode = ScrollBarVisibility.Disabled },
            new() { Title = "Optimization", Icon = PackIconLucideKind.Rocket, Content = _serviceProvider.GetRequiredService<Optimization>() },
            new() { Title = "Maintenance", Icon = PackIconLucideKind.Wrench, Content = _serviceProvider.GetRequiredService<Maintenance>() },
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
        public static void QCOpenQuickCenter()
        {

            if (MainConfiguration.Default.EnableQuickCenter)
            {
                var quickCenter = new QuickCenter();
                quickCenter.Show();
                quickCenter.Activate();
            }
        }

        [RelayCommand]
        public static void QCOpenApp()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null) return;
            if (mainWindow.Visibility != Visibility.Visible || mainWindow.WindowState == WindowState.Minimized)
            {
                if (mainWindow.Visibility != Visibility.Visible)
                {
                    mainWindow.Show();
                }

                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Activate();
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;
                mainWindow.Focus();
                WeakReferenceMessenger.Default.Send(new WindowVisibleMessage(true));
            }
            else
            {
                mainWindow.Hide();
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Visibility = Visibility.Collapsed;
                WeakReferenceMessenger.Default.Send(new WindowVisibleMessage(false));
                var windows = Application.Current.Windows;
                foreach (Window win in windows)
                {
                    if (win is not MainWindow)
                        win.Close();
                }

            }
        }

        public class WindowVisibleMessage(bool value) : ValueChangedMessage<bool>(value) { }
    }
}
