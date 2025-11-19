using Celer.ViewModels.MaintenanceVM;
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
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
