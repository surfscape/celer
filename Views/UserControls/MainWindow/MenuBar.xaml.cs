using System.Windows;
using System.Windows.Controls;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Views.UserControls.MainWindow
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml used on the MainWindow.
    /// </summary>
    [INotifyPropertyChanged]
    public partial class MenuBar : UserControl
    {
        [ObservableProperty]
        private bool schoolFeatureCheckStatus;
        public MenuBar()
        {
            InitializeComponent();
            EnableSchoolFeatureCheckbox.DataContext = this;
            SchoolFeatureCheckStatus = Properties.MainConfiguration.Default.SchoolFeature;
        }

        private void OpenAmbientChecker_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<AmbientChecker>();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Settings>();
        }

        /// <summary>
        /// Helper function that opens a specific window, prohibits opening another instance of it and has the ability to bring it to the foreground if already opened.
        /// </summary>
        /// <param name="window">Object of the desired window to open</param>
        private void OpenWindow<T>() where T : Window, new()
        {
                Window? window = Application.Current.Windows.OfType<T>().FirstOrDefault();
                if (window == null || !window.IsVisible)
                {
                    window = new T();
                    window.Owner = Application.Current.MainWindow;
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
