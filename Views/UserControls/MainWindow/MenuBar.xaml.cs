﻿using Celer.Properties;
using Celer.Services;
using Celer.Views.Windows;
using Celer.Views.Windows.Dialogs;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public MenuBar(MenuBarNavigation menuBarNavigation)
        {
            InitializeComponent();
            _menuBarNavigation = menuBarNavigation;
            NavigationMenu.DataContext = _menuBarNavigation;
            EnableSchoolFeatureCheckbox.DataContext = new SchoolDataContext();
            AboutMenu.DataContext = new AboutDataContext();
        }

        public partial class AboutDataContext : ObservableObject
        {
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
            private static void OpenAboutWindow()
            {
                OpenWindow<AboutWindow>();
            }


            public AboutDataContext() { }
        }

        public partial class SchoolDataContext : ObservableObject
        {
            private bool _isEnabled;
            public bool IsEnabled
            {
                get => _isEnabled;
                set
                {
                    if (_isEnabled == value)
                        return;

                    if (value == false)
                    {
                        _isEnabled = false;
                        MainConfiguration.Default.EnableSchoolFeatures = false;
                        MainConfiguration.Default.Save();
                        OnPropertyChanged();
                    }
                    else
                    {
                        var dialog = new SchoolKeyDialog { Owner = Application.Current.MainWindow };

                        if (dialog.ShowDialog() == true)
                        {
                            if (
                                dialog.EnteredText == Secrets.Default.SchoolProvisionKey1
                                || dialog.EnteredText == Secrets.Default.SchoolProvisionKey2
                                || dialog.EnteredText == Secrets.Default.SchoolProvisionKey3
                            )
                            {
                                _isEnabled = true;
                                MainConfiguration.Default.EnableSchoolFeatures = true;
                                MainConfiguration.Default.Save();
                                OnPropertyChanged();
                            }
                            else
                            {
                                MessageBox.Show(
                                    "A chave não está correta ou o modelo do equipamento não é compatível.",
                                    "Erro",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error
                                );

                                Application.Current.Dispatcher.InvokeAsync(() =>
                                {
                                    OnPropertyChanged(nameof(IsEnabled));
                                });
                            }
                        }
                        else
                        {
                            Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                OnPropertyChanged(nameof(IsEnabled));
                            });
                        }
                    }
                }
            }

            public SchoolDataContext() { }
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
        private static void OpenWindow<T>()
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
                if (App.AppHost?.Services.GetService(typeof(T)) is T resolvedWindow)
                {
                    window = resolvedWindow;
                }
                else
                {
                    window = Activator.CreateInstance<T>();
                }

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
                MessageBox.Show(
                    $"Erro ao abrir a janela {typeof(T).Name}:\n\n{ex}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
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

        public MenuBarNavigation(NavigationService navigationService) =>
            _navigationService = navigationService;

        [RelayCommand]
        private void NavigateToTab(string tab)
        {
            _navigationService.Navigate(tab);
        }
    }
}
