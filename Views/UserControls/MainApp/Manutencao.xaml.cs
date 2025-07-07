using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Manutenção.xaml
    /// </summary>
    public partial class Manutencao : UserControl
    {
        private readonly ManutencaoViewModel _viewModel;

        public Manutencao(ManutencaoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
