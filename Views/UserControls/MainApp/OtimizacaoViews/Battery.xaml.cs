using Celer.ViewModels.OtimizacaoVM;
using System.Windows.Controls;


namespace Celer.Views.UserControls.MainApp.OtimizacaoViews
{
    /// <summary>
    /// Interaction logic for Battery.xaml
    /// </summary>
    public partial class Battery : UserControl
    {
        public Battery(BatteryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
