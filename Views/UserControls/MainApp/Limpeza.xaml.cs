using System.Windows.Controls;
using Celer.ViewModels;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Limpeza.xaml
    /// </summary>
    public partial class Limpeza : UserControl
    {
        private readonly CleanEngine _viewModel;

        public Limpeza(CleanEngine viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
