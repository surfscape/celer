using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
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
            SurfScapeGateway surfScapeGateway = new();
            surfScapeGateway.ShowDialog();
            mainWindow.Show();
        }
        else
        {
            Onboarding setupWin = new();
            setupWin.Show();
        }
    }
}

