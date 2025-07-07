using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Otimização.xaml
    /// </summary>
    public partial class Otimizacao : UserControl
    {
        private readonly OtimizacaoViewModel _viewModel;

        public Otimizacao(OtimizacaoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
