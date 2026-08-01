using Celer.Models;
using Celer.Properties;
using Celer.Services;
using Celer.Utilities;
using Celer.ViewModels.OpsecVM;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
			TabsModule =
				[
					new() { Title = "Dashboard", Icon = PackIconLucideKind.SquareActivity, Content = _serviceProvider.GetRequiredService<DashboardViewModel>(), VerticalScrollMode = ScrollBarVisibility.Disabled, NavigationKey = NavigationTabKey.Dashboard },
					new() { Title = "Cleaning", Icon = PackIconLucideKind.Trash, Content = _serviceProvider.GetRequiredService<CleanEngine>(), VerticalScrollMode = ScrollBarVisibility.Disabled, NavigationKey = NavigationTabKey.Cleaning },
					new() { Title = "Optimization", Icon = PackIconLucideKind.Rocket, Content = _serviceProvider.GetRequiredService<OptimizationViewModel>(), NavigationKey = NavigationTabKey.Optimization },
					new() { Title = "Maintenance", Icon = PackIconLucideKind.Wrench, Content = _serviceProvider.GetRequiredService<MaintenanceViewModel>(), NavigationKey = NavigationTabKey.Maintenance },
					new() { Title = "Privacy & Security", Icon = PackIconLucideKind.Shield, Content = _serviceProvider.GetRequiredService<OverviewViewModel>(), NavigationKey = NavigationTabKey.PrivacySecurity }
				];
			foreach (var module in TabsModule)
			{
				if (module.Content is not null)
					_navigationService.RegisterTab(module.NavigationKey, module.Content);
			}

			Application.Current.Dispatcher.InvokeAsync(
				() => RequestNavigation(TabsModule[SelectedTabIndex].NavigationKey),
				DispatcherPriority.Loaded);
		}

		private void RequestNavigation(NavigationTabKey tabKey)
		{
			Application.Current.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					// Resolved here rather than at queue time: an explicit Navigate(tab, subview)
					// sets SelectedTabIndex first, so this callback is queued before the subview
					// has been pushed. Reading it now sees the settled state and the guard in
					// NavigateInternal turns this into a no-op instead of resetting to Main.
					var innerView = _navigationService.GetInnerViewForTab(tabKey);
					await _navigationService.NavigateInternal(tabKey, innerView);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Navigation to {tabKey} failed: {ex}");
				}
			});
		}

		private void OnCompactModeChanged(object? sender, bool isCompact)
		{
			TabControlCompactMode = isCompact;
		}

		private async Task NavigateTo(NavigationTabKey tabKey, string? subview)
		{
			var tab = TabsModule.FirstOrDefault(tb => tb.NavigationKey == tabKey);
			if (tab != null)
			{
				SelectedTabIndex = TabsModule.IndexOf(tab);
				await _navigationService.NavigateInternal(tabKey, subview);
			}
		}
		partial void OnSelectedTabIndexChanged(int value)
		{
			if (value < 0 || value >= TabsModule.Count)
				return;

			var tabKey = TabsModule[value].NavigationKey;
			if (tabKey == default)
				return;

			RequestNavigation(tabKey);
		}

		[RelayCommand]
		private async Task NavigateToTab(string tab)
		{
			var found = TabsModule.FirstOrDefault(t => t.Title == tab);
			if (found != null)
				await _navigationService.Navigate(found.NavigationKey);
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

		private void OnNavigationChanged(NavigationTabKey? tab, string? innerView)
		{
			CanGoBack = !string.IsNullOrEmpty(innerView) && !string.Equals(innerView, "Main", StringComparison.Ordinal);
		}


		[RelayCommand]
		private static void CloseApp()
		{
			Application.Current.Shutdown();
		}

		[RelayCommand]
		private void OpenWindow(string window)
		{
			switch (window)
			{
				case "Settings":
					OpenWindow<Settings>(); break;
				case "Ambient":
					OpenWindow<AmbientChecker>(); break;
				case "About":
					OpenWindow<AboutWindow>(); break;
			}
		}

		[RelayCommand]
		private static void OpenLink(string url)
		{
			HyperlinkExtensions.OpenLink(url);
		}

		/// <summary>
		/// Helper function that opens a specific window, prohibits opening another instance of it and has the ability to bring it to the foreground if already opened.
		/// </summary>
		/// <param name="window">Object of the desired window to open</param>
		private void OpenWindow<T>() where T : Window
		{
			var existing = Application.Current.Windows.OfType<T>().FirstOrDefault();
			if (existing is not null)
			{
				if (existing.WindowState == WindowState.Minimized)
					existing.WindowState = WindowState.Normal;

				existing.Show();        // covers the hidden case
				existing.Activate();
				return;
			}

			var window = _serviceProvider.GetService<T>() ?? Activator.CreateInstance<T>();

			var owner = Application.Current.MainWindow;
			if (owner is not null && owner != window && owner.IsVisible)
				window.Owner = owner;

			window.Show();
		}
		public partial class QCMenuViewModel : ObservableObject
		{
			[RelayCommand]
			private static void QCExitApp()
			{
				Application.Current.Shutdown();
			}

			[RelayCommand]
			public static void QCOpenApp()
			{
				var mainWindow = Application.Current.MainWindow;
				if (MainConfiguration.Default.EnableQuickCenter)
				{
					var quickCenter = new QuickCenter();
					quickCenter.Show();
					quickCenter.Activate();
				}
				else
				{
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
			}
		}
		public class WindowVisibleMessage(bool value) : ValueChangedMessage<bool>(value) { }
	}
}