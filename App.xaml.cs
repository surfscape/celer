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
using Celer.ViewModels.SubViews;

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
            .ConfigureServices((context, services) =>
            {
                // register main services, these include services that are used across the application and also the main window
                services.AddSingleton<MainWindow>();
                services.AddSingleton<NavigationService>();
                services.AddSingleton<MainWindowViewModel>();

                // viewmodels for windows, tabs, and viewmodel (incl. usercontrols)
                services.AddTransient<MenuBarNavigation>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<CleanEngine>();
                services.AddTransient<OtimizacaoViewModel>();
                services.AddTransient<MemoryManagement>();
                services.AddTransient<ManutencaoViewModel>();
                services.AddTransient<Battery>();
                services.AddTransient<Sensors>();
                services.AddTransient<SensorViewModel>();
                services.AddTransient<PrivacidadeViewModel>();
                


                // usercontrols themselves (and other views that need access to the services)
                services.AddTransient<Dashboard>();
                services.AddTransient<Limpeza>();
                services.AddTransient<Otimizacao>();
                services.AddTransient<Manutencao>();
                services.AddTransient<Privacidade>();
                services.AddTransient<MenuBar>();
            })
            .Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Celer.Properties.MainConfiguration.Default.Reset();
        bool onboarding = Celer.Properties.MainConfiguration.Default.HasUserDoneSetup;

        if (AppHost == null)
        {
            MessageBox.Show("Erro ao inicializar AppHost. Por favor, reinicie a aplicação ou tenta fazer a sua reinstalação", "Erro de infrastutura", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new InvalidOperationException("AppHost não foi inicializado");
        }
        if (onboarding)
        {
            var onboardingWindow = new Onboarding();
            onboardingWindow.Show();
        }
        var surfScapeGateway = new SurfScapeGateway();
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        surfScapeGateway.ShowDialog();
        mainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if(AppHost != null)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            AppHost = null;
            base.OnExit(e);
        }
        base.OnExit(e);
    }
}

