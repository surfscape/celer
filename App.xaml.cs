using Celer.Views.Windows;
using System.Windows;

namespace Celer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Celer.Properties.MainConfiguration.Default.Reset();
        bool onboarding = Celer.Properties.MainConfiguration.Default.HasUserDoneSetup;
        if (!onboarding)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
        }
        else
        {
            Onboarding setupWin = new();
            setupWin.Show();
        }
    }
}

