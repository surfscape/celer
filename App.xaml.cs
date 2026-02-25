using Celer.Models.Preferences;
using Celer.Properties;
using Celer.Services;
using Celer.ViewModels;
using Celer.ViewModels.MaintenanceVM;
using Celer.ViewModels.OptimizationVM;
using Celer.Views.UserControls.MainApp;
using Celer.Views.UserControls.MainApp.MaintenanceViews;
using Celer.Views.UserControls.MainApp.OptimizationViews;
using Celer.Views.UserControls.MainWindow;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Celer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    private Mutex? _singleInstanceMutex;

    public App()
    {
        if (MainConfiguration.Default.EnableSentry)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            SentrySdk.Init(o =>
            {
                o.Dsn = "";
                o.Debug = true;
                o.TracesSampleRate = 1.0;
            });
        }
        MainConfiguration.Default.PropertyChanged += OnSettingsChanged;
    }

    void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        SentrySdk.CaptureException(e.Exception);
        e.Handled = true;
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
        if (Environment.OSVersion.Version.Build <= 22000)
        {
            if (Current.ThemeMode == ThemeMode.System && IsLightLegacyTheme())
                Current.Resources["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            else if(Current.ThemeMode == ThemeMode.System && !IsLightLegacyTheme())
                Current.Resources["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));

            if (Current.ThemeMode == ThemeMode.Light)
                Current.Resources["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            else if (Current.ThemeMode == ThemeMode.Dark)
                Current.Resources["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));


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
        if(!e.Args.Contains("-disableMutexProtection")) { 
        using (_singleInstanceMutex = new Mutex(true, "Celer", out bool createdNew))
        {
            if (!createdNew)
            {
                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
                Shutdown();
            }
        }
        }

        AppHost = Host.CreateDefaultBuilder()
    .ConfigureServices(
        (context, services) =>
        {
            // register main services, these include services that are used across the application and the main window
            services.AddTransient<SurfScapeGateway>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<NavigationService>();
            services.AddSingleton<MainWindowViewModel>();


            // viewmodels for the user controls
            services.AddSingleton<MenuBarNavigation>();
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<CleanEngine>();
            services.AddSingleton<OptimizationViewModel>();
            services.AddTransient<MemoryViewModel>();
            services.AddTransient<BatteryViewModel>();
            services.AddTransient<VideoViewModel>();
            services.AddTransient<SensorViewModel>();
            services.AddSingleton<MaintenanceViewModel>();
            services.AddSingleton<RepairViewModel>();
            services.AddTransient<NetworkViewModel>();

            // usercontrols themselves (and other views that need access to the services)
            services.AddSingleton<MenuBar>();
            services.AddSingleton<Dashboard>();
            services.AddSingleton<Limpeza>();
            services.AddSingleton<Optimization>();
            services.AddTransient<MemoryManagement>();
            services.AddTransient<Battery>();
            services.AddTransient<Video>();
            services.AddTransient<Sensors>();
            services.AddSingleton<Maintenance>();
            services.AddTransient<Repair>();
            services.AddTransient<Network>();
            services.AddSingleton<Privacidade>();
        }
    )
    .Build();
        bool hasUserDoneSetup = MainConfiguration.Default.HasUserDoneSetup;
        if (AppHost is not null)
        {
            if (!e.Args.Contains("-silent") && hasUserDoneSetup)
            {
                SetFluentTheme();
                LegacyTheme();
                var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
                surfScapeGateway.MainWindowTrigger = true;
                surfScapeGateway.ShowDialog();
                if (MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.PreferHardware || MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.Auto)
                    RenderOptions.ProcessRenderMode = RenderMode.Default;
                else if (MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.PreferSoftware)
                    RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
            else if (!e.Args.Contains("-silent") && !hasUserDoneSetup)
            {
                SetFluentTheme();
                LegacyTheme();
                var onboardingWindow = new Onboarding();
                onboardingWindow.Show();
            }
            else if (e.Args.Contains("-silent"))
            {
                SetFluentTheme();
                LegacyTheme();
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
            base.OnExit(e);
        }
        base.OnExit(e);
    }
}
