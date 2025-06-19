using System.Diagnostics;
using System.Security.Policy;
using System.Windows;
using System.Windows.Navigation;
using Celer.Views.Windows.Utils;
using Microsoft.Extensions.DependencyInjection;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TermsCheckbox.IsChecked == false)
            {
                TermsCheckbox.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                Properties.MainConfiguration.Default.HasUserDoneSetup = true;
                Properties.MainConfiguration.Default.Save();

                var gateway = App.AppHost?.Services.GetService<SurfScapeGateway>();
                gateway.MainWindowTrigger = true;
                gateway?.ShowDialog();

                Close();
            }
        }
    }
}
