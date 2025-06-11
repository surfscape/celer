using Celer.Interfaces;
using Celer.ViewModels.OtimizacaoVM;
using System.Windows;
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
            Loaded += Sensors_Loaded;
        }

        private async void Sensors_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (_viewModel.IsLoading)
            {
                await _viewModel.Initialize();
            }
        }

        public void OnNavigatedTo()
        {
            if(!_viewModel.IsLoading)
            {
                _viewModel.StartTimer();
            }

        }

        public void OnNavigatedFrom()
        {
            _viewModel.StopTimer();
        }
    }
}
