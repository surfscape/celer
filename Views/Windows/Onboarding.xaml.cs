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
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }
    }
}
