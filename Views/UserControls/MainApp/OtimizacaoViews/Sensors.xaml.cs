using Celer.Interfaces;
using Celer.ViewModels.OtimizacaoVM;
using System.Windows.Controls;


namespace Celer.Views.UserControls.MainApp.OtimizacaoViews
{
    /// <summary>
    /// Interaction logic for Sensors.xaml
    /// </summary>
    public partial class Sensors : UserControl, INavigationAware
    {
        private readonly SensorViewModel _viewModel;

        public Sensors(SensorViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        public void OnNavigatedTo()
        {
            _viewModel.StartTimer();
        }

        public void OnNavigatedFrom()
        {
            _viewModel.StopTimer();
        }
    }
}
