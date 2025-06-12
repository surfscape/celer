using System.ComponentModel;
using System.Windows;
using Celer.ViewModels;

namespace Celer.Views.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private readonly SettingsViewModel _viewModel;

        public Settings()
        {
            InitializeComponent();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            async Task<MessageBoxResult> value(
                string title,
                string message,
                MessageBoxButton buttons,
                MessageBoxImage icon
            )
            {
                return MessageBox.Show(this, message, title, buttons, icon);
            }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            _viewModel = new SettingsViewModel
            {
                ShowDialogAsync = value,
                CloseWindowAction = () =>
                {
                    Close();
                },
            };

            this.DataContext = _viewModel;
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_viewModel == null || e.Cancel)
            {
                return;
            }

            if (IsVisible)
            {
                bool canClose = await _viewModel.HandleWindowCloseRequestAsync();
                if (!canClose)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
