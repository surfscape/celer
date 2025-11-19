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
using System.Diagnostics;
using System.Windows;

namespace Celer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
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
                    services.AddSingleton<PrivacidadeViewModel>();

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
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        // closes celer if another instance is already running
        using (var mutex = new Mutex(true, "Celer", out bool createdNew))
        {
            if (!createdNew)
            {
                return;
            }
        }

        if (AppHost == null)
        {
            MessageBox.Show(
                "Error while initializing AppHost. Please try to restart or reinstall Celer from an official source.",
                "Infrastructure Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            throw new InvalidOperationException("AppHost not initialized");
        }

        bool hasUseDoneSetup = Celer.Properties.MainConfiguration.Default.HasUserDoneSetup;
        var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
        if (!e.Args.Contains("-silent"))
        {

            if (!hasUseDoneSetup)
            {
                var onboardingWindow = new Onboarding();
                onboardingWindow.Show();
            }
            else
            {
                surfScapeGateway.MainWindowTrigger = true;
                surfScapeGateway.ShowDialog();
            }
        }
        else
        {
            /* TODO: Implement silent mode logic, maybe with NotifyIcon to provide a lightweight system tray interface for Celer */
            MessageBox.Show("Celer is running in silent mode.");
            Process[] processes = Process.GetProcessesByName("Celer");
            foreach (Process process in processes)
            {
                process.Kill();
            }
        }
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            AppHost = null;
            base.OnExit(e);
        }
        base.OnExit(e);
    }
}
