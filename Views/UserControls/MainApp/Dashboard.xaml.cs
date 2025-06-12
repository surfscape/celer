using System.Windows;
using System.Windows.Controls;
using Celer.ViewModels;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        private readonly DashboardViewModel _viewModel;

        public Dashboard(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += Dashboard_Loaded;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (_viewModel.IsLoading)
            {
                await _viewModel.InitializeAsync();
            }
        }
    }
}
