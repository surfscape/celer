using Celer.Utilities;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Celer.Views.Windows
{
    /// <summary>
    /// Interaction logic for Onboarding.xaml
    /// </summary>
    public partial class Onboarding : Window
    {
        public Onboarding()
        {
            InitializeComponent();
            OnboardingOptions.DataContext = new OnboardingViewModel { OnCompleted = Close };
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true }
                );
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine($"Failed to open link {ex.Message}");
            }
        }

        public partial class OnboardingViewModel : ObservableObject
        {
            public Action? OnCompleted { get; set; }

            [ObservableProperty]
            private bool acceptTerms = false;

            [ObservableProperty]
            private bool autoUpdates = false;

            [ObservableProperty]
            private bool autoStartup = false;

            [ObservableProperty]
            private bool enableSentry = false;

            [RelayCommand]
            private void Start()
            {
                Properties.MainConfiguration.Default.HasUserDoneSetup = AcceptTerms;
                Properties.MainConfiguration.Default.EnableAutoSurfScapeGateway = AutoUpdates;
                Properties.MainConfiguration.Default.AutoStartup = AutoStartup;
                Properties.MainConfiguration.Default.CloseShouldMinimize = AutoStartup;
                Properties.MainConfiguration.Default.EnableSentry = EnableSentry;
                Properties.MainConfiguration.Default.Save();

                if (Properties.MainConfiguration.Default.AutoStartup)
                    UserLand.SetAutoStartup();

                if (EnableSentry)
                {
                   Process.Start(Application.ResourceAssembly.Location, "-disableMutexProtection");
                    Process.GetCurrentProcess().Kill();
                }
                var gateway = App.AppHost?.Services.GetService<SurfScapeGateway>();
                if (gateway is not null)
                {
                    gateway.MainWindowTrigger = true;
                    OnCompleted?.Invoke();
                    gateway.ShowDialog();

                }
            }
        }
    }
}
