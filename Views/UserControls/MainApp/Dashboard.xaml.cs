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
        }
    }
}
