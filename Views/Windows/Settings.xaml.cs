using Celer.ViewModels;
using System.ComponentModel;
using System.Windows;

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

            _viewModel = new SettingsViewModel
            {
                ShowDialogAsync = async (title, message, buttons, icon) =>
                {
                    return MessageBox.Show(this, message, title, buttons, icon);
                },

                CloseWindowAction = () =>
                    {
                        this.Close();
                    }
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
