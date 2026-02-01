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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

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
        MainConfiguration.Default.PropertyChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (MainConfiguration.Default.Theme == (int)CelerTheme.Light)
        {
            Current.ThemeMode = ThemeMode.Light;
        }
        else if (MainConfiguration.Default.Theme == (int)CelerTheme.Dark)
        {
            Current.ThemeMode = ThemeMode.Dark;
        }
        else
        {
            Current.ThemeMode = ThemeMode.System;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // closes celer if another instance is already running
        using (_singleInstanceMutex = new Mutex(true, "Celer", out bool createdNew))
        {
            if (!createdNew)
            {
                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
                Shutdown();
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
        if (!e.Args.Contains("-silent") && hasUserDoneSetup)
        {
            if (AppHost != null)
            {
                var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
                surfScapeGateway.MainWindowTrigger = true;
                surfScapeGateway.ShowDialog();
                if (MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.PreferHardware || MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.Auto)
                    RenderOptions.ProcessRenderMode = RenderMode.Default;
                else if (MainConfiguration.Default.GraphicRenderingMode == (int)CelerRenderMode.PreferSoftware)
                    RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            }
            else
            {
                throw new InvalidOperationException("AppHost not initialized");
            }
        }
        else if (!hasUserDoneSetup)
        {
            var onboardingWindow = new Onboarding();
            onboardingWindow.Show();
        }
        else
        {
            var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
            surfScapeGateway.MainWindowTrigger = true;
            surfScapeGateway.SilentStartup = true;
            surfScapeGateway.ShowDialog();
        }
        if(e.Args.Contains("-silent") && !hasUserDoneSetup)
        {
            Debug.WriteLine("The launch option -silent can't be used without finishing the onboarding first!");
        }
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
        base.OnExit(e);
    }
}
