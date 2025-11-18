using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Otimização.xaml
    /// </summary>
    public partial class Optimization : UserControl
    {
        private readonly OptimizationViewModel _viewModel;

        public Optimization(OptimizationViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
