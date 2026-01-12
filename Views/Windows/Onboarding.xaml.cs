using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
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
            OnboardingOptions.DataContext = new OnboardingViewModel { IsDone = Close };
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true }
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao abrir link: " + ex.Message);
            }
        }

        public partial class OnboardingViewModel : ObservableObject
        {
            public Action? IsDone { get; set; }

            [ObservableProperty]
            private bool acceptTerms = false;

            [ObservableProperty]
            private bool autoUpdates = false;

            [ObservableProperty]
            private bool autoStartup = false;

            [RelayCommand]
            private void Start(string url)
            {
                Properties.MainConfiguration.Default.HasUserDoneSetup = AcceptTerms;
                Properties.MainConfiguration.Default.EnableAutoSurfScapeGateway = AutoUpdates;
                Properties.MainConfiguration.Default.AutoStartup = AutoStartup;
                Properties.MainConfiguration.Default.CloseShouldMinimize = AutoStartup;
                Properties.MainConfiguration.Default.Save();

                var gateway = App.AppHost?.Services.GetService<SurfScapeGateway>();
                if (gateway is not null)
                {
                    gateway.MainWindowTrigger = true;
                    IsDone?.Invoke();
                    gateway?.ShowDialog();

                }
            }
        }
    }
}
