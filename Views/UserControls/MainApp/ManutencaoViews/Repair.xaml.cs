using Celer.ViewModels.ManutencaoVM;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp.ManutencaoViews
{
    /// <summary>
    /// Interaction logic for Repair.xaml
    /// </summary>
    public partial class Repair : UserControl
    {
        private readonly RepairViewModel _viewModel;

        public Repair(RepairViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
