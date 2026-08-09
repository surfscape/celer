using Celer.Models.Preferences;
using Celer.Properties;
using Celer.Services;
using Celer.ViewModels;
using Celer.ViewModels.MaintenanceVM;
using Celer.ViewModels.OpsecVM;
using Celer.ViewModels.OptimizationVM;
using Celer.ViewModels.SoftwareVM;
using Celer.Views.Pages.Settings;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using static Celer.Views.Pages.Settings.SettingsAdvancedViewModel;

namespace Celer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private const uint HWND_BROADCAST = 0xffff;
	public static readonly uint WH_SHOWME = PInvoke.RegisterWindowMessage("WM_SHOWME");
	private Mutex? _singleInstanceMutex;
	public static IHost? AppHost { get; private set; }

	public App()
	{
		/* event for windows 10 to check when a user preference is changed to trigger a theme update */
		if (Environment.OSVersion.Version.Build <= 26000)
			SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
		MainConfiguration.Default.PropertyChanged += OnSettingsChanged;
		WeakReferenceMessenger.Default.Register<TriggerApplicationClosureMessage>(this, (r, m) =>
		{
			if (Environment.ProcessPath is not null)
				Process.Start(Environment.ProcessPath, "--disableMutexProtection");
			Current.Shutdown();
		});
		string langKey = MainConfiguration.Default.Language;
		if (string.IsNullOrEmpty(langKey))
		{
			langKey = "en";
		}
		var culture = new CultureInfo(langKey);
		Thread.CurrentThread.CurrentCulture = culture;
		Thread.CurrentThread.CurrentUICulture = culture;
		CultureInfo.DefaultThreadCurrentCulture = culture;
		CultureInfo.DefaultThreadCurrentUICulture = culture;
	}

	private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		switch (e.Category)
		{
			case UserPreferenceCategory.General: // TODO: maybe using the Color category might be better but needs testing
				LegacyTheme();
				break;
		}
	}
	private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
	{
		SetFluentTheme();
		LegacyTheme();
	}

	private static void SetFluentTheme()
	{
		Current.ThemeMode = MainConfiguration.Default.Theme == (int)CelerTheme.Light ? ThemeMode.Light : MainConfiguration.Default.Theme == (int)CelerTheme.Dark ? ThemeMode.Dark : ThemeMode.System;
	}

	/// <summary>
	/// Set window background to a static color depending on the theme. This is used if the OS is on the latest or older version of Windows 10, since Windows 10 does not support Mica.
	/// </summary>
	private static void LegacyTheme()
	{
		if (Environment.OSVersion.Version.Build <= 26000)
		{
			if ((Current.ThemeMode == ThemeMode.System && IsLightLegacyTheme()) || Current.ThemeMode == ThemeMode.Light)
				Current.Resources["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
			else
				Current.Resources["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
		}
	}

	private static bool IsLightLegacyTheme()
	{
		using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
		var value = key?.GetValue("AppsUseLightTheme");
		return value is int i && i > 0;
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		// closes celer if another instance is already running
		if (!e.Args.Contains("--disableMutexProtection"))
		{
			_singleInstanceMutex = new Mutex(true, "Celer");
			if (!_singleInstanceMutex.WaitOne(TimeSpan.Zero, true))
			{
				PInvoke.PostMessage((HWND)HWND_BROADCAST, WH_SHOWME, 0, 0);
				_singleInstanceMutex.Dispose();
				_singleInstanceMutex = null;
				Shutdown();
				return;
			}
		}

		AppHost = Host.CreateDefaultBuilder()
	.ConfigureServices(
		(context, services) =>
		{
			// register main services, these include services that are used across the application and the main window
			services.AddSingleton<NavigationService>();
			services.AddSingleton<MainWindowViewModel>();
			services.AddSingleton<QuickCenterViewModel>();
			services.AddSingleton<MainWindow>();
			services.AddTransient<SurfScapeGateway>();
			services.AddSingleton<SettingsNavigation>();
			services.AddTransient<Settings>();

			// viewmodels for pages and views
			services.AddSingleton<DashboardViewModel>();
			services.AddSingleton<CleanEngine>();
			services.AddSingleton<OptimizationViewModel>();
			services.AddTransient<MemoryViewModel>();
			services.AddTransient<BatteryViewModel>();
			services.AddTransient<VideoViewModel>();
			services.AddTransient<SensorViewModel>();
			services.AddTransient<SoftwareViewModel>();
			services.AddTransient<PackageManagerViewModel>();
			services.AddSingleton<MaintenanceViewModel>();
			services.AddSingleton<RepairViewModel>();
			services.AddTransient<NetworkViewModel>();
			services.AddTransient<OverviewViewModel>();
			services.AddSingleton<SettingsViewModel>();
			services.AddTransient<SettingsBaseViewModel>();
			services.AddTransient<SettingsShellViewModel>();
			services.AddTransient<SettingsGeneralViewModel>();
			services.AddTransient<SettingsInterfaceViewModel>();
			services.AddTransient<SettingsModuleCleaningViewModel>();
			services.AddTransient<SettingsAdvancedViewModel>();
		}
	)
	.Build();
		bool hasUserDoneSetup = MainConfiguration.Default.HasUserDoneSetup;
		if (AppHost is not null)
		{
			if (MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.PreferHardware || MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.Auto)
				RenderOptions.ProcessRenderMode = RenderMode.Default;
			else if (MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.PreferSoftware)
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			SetFluentTheme();
			LegacyTheme();
			if (!e.Args.Contains("--silent") && hasUserDoneSetup)
			{
				var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
				surfScapeGateway.MainWindowTrigger = true;
				surfScapeGateway.ShowDialog();
			}
			else if (!e.Args.Contains("--silent") && !hasUserDoneSetup)
			{
				var onboardingWindow = new Onboarding();
				onboardingWindow.Show();
			}
			else if (e.Args.Contains("--silent"))
			{
				var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
				surfScapeGateway.MainWindowTrigger = true;
				surfScapeGateway.SilentStartup = true;
				surfScapeGateway.ShowDialog();
			}
			else
				Debug.WriteLine("The launch option -silent can't be used without finishing the onboarding first!");
		}
		else
			throw new InvalidOperationException("AppHost not initialized!");

		base.OnStartup(e);
	}


	protected override async void OnExit(ExitEventArgs e)
	{
		if (AppHost is not null)
		{
			await AppHost.StopAsync();
			AppHost.Dispose();
		}
		_singleInstanceMutex?.ReleaseMutex();
		base.OnExit(e);
	}
}
