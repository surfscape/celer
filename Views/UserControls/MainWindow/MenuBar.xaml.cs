using Celer.Properties;
using Celer.Services;
using Celer.Views.Windows;
using Celer.Views.Windows.Dialogs;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainWindow
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml used on the MainWindow.
    /// </summary>
    public partial class MenuBar : UserControl
    {
        private readonly MenuBarNavigation _menuBarNavigation;
        public MenuBar(MenuBarNavigation menuBarNavigation)
        {
            InitializeComponent();
            _menuBarNavigation = menuBarNavigation;
            NavigationMenu.DataContext = _menuBarNavigation;
            EnableSchoolFeatureCheckbox.DataContext = new SchoolDataContext();
        }

        public partial class SchoolDataContext : ObservableObject
        {
            [ObservableProperty]
            private bool isEnabled = MainConfiguration.Default.EnableSchoolFeatures;

            public SchoolDataContext () {
            }

            [RelayCommand]
            private void ToggleSchoolFeature()
            {
                if(MainConfiguration.Default.EnableSchoolFeatures)
                {
                    MainConfiguration.Default.EnableSchoolFeatures = false;
                    MainConfiguration.Default.Save();
                }
                else
                {
                    var dialog = new SchoolKeyDialog
                    {
                        Owner = Application.Current.MainWindow
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        if (dialog.EnteredText == "a-tua-chave-correta")
                        {
                            MainConfiguration.Default.EnableSchoolFeatures = true;
                            MainConfiguration.Default.Save();
                        }
                        else
                        {
                            MessageBox.Show("A chave não está correta ou o model do equipamento não é compatível.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                
            }

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

    public partial class MenuBarNavigation : ObservableObject
    {
        private readonly NavigationService _navigationService;

        public MenuBarNavigation(NavigationService navigationService) => _navigationService = navigationService;

        [RelayCommand]
        private void NavigateToTab(string tab)
        {
            _navigationService.Navigate(tab);
        }
    }

}
