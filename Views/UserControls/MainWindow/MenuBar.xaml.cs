using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Celer.Views.UserControls.MainWindow
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml used on the MainWindow.
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
            EnableSchoolFeatureCheckbox.IsEnabled = Properties.MainConfiguration.Default.SchoolFeature;
        }

        private void EnableSchoolFeatureCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.MainConfiguration.Default.SchoolFeature = EnableSchoolFeatureCheckbox.IsChecked;
            Properties.MainConfiguration.Default.Save();
        }

        private void OpenAmbientChecker_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<AmbientChecker>();
        }

        private void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<SurfScapeGateway>();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Settings>();
        }

        /// <summary>
        /// Helper function that opens a specific window, prohibits opening another instance of it and has the ability to bring it to the foreground if already opened.
        /// </summary>
        /// <param name="window">Object of the desired window to open</param>
        private static void OpenWindow<T>() where T : Window, new()
        {
                Window? window = Application.Current.Windows.OfType<T>().FirstOrDefault();
                if (window == null || !window.IsVisible)
                {
                window = new T
                {
                    Owner = Application.Current.MainWindow
                };
                window.ShowDialog();
                    window.Closed += (s, args) => window = null;
                }
                else
                {
                    window.Activate();
                }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
