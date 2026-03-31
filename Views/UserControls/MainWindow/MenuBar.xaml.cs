using Celer.Services;
using Celer.Views.Windows;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Mono.Unix.Native;
using System.Diagnostics;
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

        public readonly IServiceProvider _serviceProvider;
        public MenuBar(MenuBarNavigation menuBarNavigation, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _menuBarNavigation = menuBarNavigation;
            _serviceProvider = serviceProvider;
            NavigationMenu.DataContext = _menuBarNavigation;
            MMenu.DataContext = new AboutDataContext(this); // pass the MenuBar instance
        }

        public partial class AboutDataContext : ObservableObject
        {
            private readonly MenuBar _menuBar;

            public AboutDataContext(MenuBar menuBar)
            {
                _menuBar = menuBar;
            }

            [RelayCommand]
            private void OpenLink(string url)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    try
                    {
                        Process.Start(
                            new ProcessStartInfo { FileName = url, UseShellExecute = true }
                        );
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Erro ao abrir link: " + ex.Message);
                    }
                }
            }

            [RelayCommand]
            private void OpenAboutWindow()
            {
                // Use the MenuBar instance to call the instance method
                _menuBar.OpenWindow<AboutWindow>();
            }


            public AboutDataContext() { }
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
            Settings window = _serviceProvider.GetRequiredService<Settings>();
            window.ShowDialog();
        }


        /// <summary>
        /// Helper function that opens a specific window, prohibits opening another instance of it and has the ability to bring it to the foreground if already opened.
        /// </summary>
        /// <param name="window">Object of the desired window to open</param>
        private void OpenWindow<T>()
            where T : Window
        {
            try
            {
                var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();
                if (existingWindow != null && existingWindow.IsVisible)
                {
                    existingWindow.Activate();
                    return;
                }
                T? window = null;
                //window = App.AppHost?.Services.GetService(typeof(T)) is T resolvedWindow ? resolvedWindow : window = Activator.CreateInstance<T>();
                window ??= _serviceProvider.GetService<T>() ?? Activator.CreateInstance<T>();
                if (
                    Application.Current.MainWindow != null
                    && Application.Current.MainWindow != window
                )
                {
                    window.Owner = Application.Current.MainWindow;
                }

                window.Closed += (s, args) => window = null;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    public partial class MenuBarNavigation : ObservableObject
    {

    }
}
