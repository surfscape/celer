using Celer.Services;
using Celer.ViewModels;
using Celer.ViewModels.ManutencaoVM;
using Celer.ViewModels.OtimizacaoVM;
using Celer.Views.UserControls.MainApp;
using Celer.Views.UserControls.MainApp.ManutencaoViews;
using Celer.Views.UserControls.MainApp.OtimizacaoViews;
using Celer.Views.UserControls.MainWindow;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<NavigationService>();
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddTransient<SurfScapeGateway>();

                    // viewmodels for the user controls
                    services.AddTransient<MenuBarNavigation>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<CleanEngine>();

                    services.AddTransient<OtimizacaoViewModel>();
                    services.AddTransient<MemoryViewModel>();
                    services.AddTransient<BatteryViewModel>();
                    services.AddTransient<VideoViewModel>();
                    services.AddTransient<SensorViewModel>();
                    services.AddTransient<ManutencaoViewModel>();
                    services.AddTransient<RepairViewModel>();
                    services.AddTransient<NetworkViewModel>();
                    services.AddTransient<PrivacidadeViewModel>();
                    services.AddSingleton<AdvancedViewModel>();

                    // usercontrols themselves (and other views that need access to the services)
                    services.AddTransient<MenuBar>();
                    services.AddTransient<Dashboard>();
                    services.AddTransient<Limpeza>();
                    services.AddTransient<Otimizacao>();
                    services.AddTransient<MemoryManagement>();
                    services.AddTransient<Battery>();
                    services.AddTransient<Video>();
                    services.AddTransient<Sensors>();
                    services.AddTransient<Manutencao>();
                    services.AddTransient<Repair>();
                    services.AddTransient<Realtek>();
                    services.AddTransient<Network>();
                    services.AddTransient<Privacidade>();
                    services.AddTransient<Advanced>();
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
                "Erro ao inicializar AppHost. Por favor reinicie a aplicação ou tente fazer a sua reinstalação",
                "Erro de infrastutura",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            throw new InvalidOperationException("AppHost não foi inicializado");
        }

        bool hasUseDoneSetup = Celer.Properties.MainConfiguration.Default.HasUserDoneSetup;

        if (!hasUseDoneSetup)
        {
            var onboardingWindow = new Onboarding();
            onboardingWindow.Show();
        }
        else
        {
            var surfScapeGateway = AppHost.Services.GetRequiredService<SurfScapeGateway>();
            surfScapeGateway.MainWindowTrigger = true;
            surfScapeGateway.ShowDialog();
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
