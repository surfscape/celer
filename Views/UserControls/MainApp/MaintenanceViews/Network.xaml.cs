using Celer.ViewModels.MaintenanceVM;
using System.Windows;
using System.Windows.Controls;


namespace Celer.Views.UserControls.MainApp.MaintenanceViews
{
    /// <summary>
    /// Interaction logic for Network.xaml
    /// </summary>
    public partial class Network : UserControl
    {
        private readonly NetworkViewModel _viewModel;
        public Network(NetworkViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
            InitializeComponent();
            Loaded += NetworkLoaded;
        }

        private async void NetworkLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.UpdatePing();
        }
    }
}
