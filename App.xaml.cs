using Celer.Services;
using Celer.ViewModels;
using Celer.Views.UserControls.MainWindow;
using Celer.Views.UserControls.MainApp;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Celer.Views.UserControls.MainApp.SubOtimização;
using Celer.Views.UserControls.MainApp.SubManutencao;

namespace Celer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    public static IHost AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<NavigationService>();

                services.AddSingleton<MainWindowViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<LimpezaViewModel>();
                services.AddTransient<OtimizacaoViewModel>();
                services.AddTransient<MemoryManagement>();
                services.AddTransient<ManutencaoViewModel>();
                services.AddTransient<Battery>();
                services.AddTransient<PrivacidadeViewModel>();
                services.AddTransient<MenuBarNavigation>();

                services.AddSingleton<MainWindow>();

                services.AddTransient<Dashboard>();
                services.AddTransient<Limpeza>();
                services.AddTransient<Otimizacao>();
                services.AddTransient<Manutencao>();
                services.AddTransient<Privacidade>();
                services.AddTransient<MenuBar>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        Celer.Properties.MainConfiguration.Default.Reset();
        bool onboarding = Celer.Properties.MainConfiguration.Default.HasUserDoneSetup;

        if (!onboarding)
        {
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            var surfScapeGateway = new SurfScapeGateway();
            mainWindow.DataContext = AppHost.Services.GetRequiredService<MainWindowViewModel>();
            surfScapeGateway.ShowDialog();
            mainWindow.Show();
        }
        else
        {
            var onboardingWindow = AppHost.Services.GetRequiredService<Onboarding>();
            onboardingWindow.Show();
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        base.OnExit(e);
    }
}

