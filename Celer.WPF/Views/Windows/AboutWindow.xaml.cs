using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Celer.Views.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        readonly string infoVersion = Assembly
    .GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
    ?.InformationalVersion.Split('+')[0] ?? "Versão N/A";
        public AboutWindow()
        {
            InitializeComponent();
            Version.Text = infoVersion;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
