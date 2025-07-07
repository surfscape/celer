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

        public async Task OnNavigatedTo()
        {
            if (!_viewModel.IsLoading)
            {
                await _viewModel.StartTimer();
            }
        }

        public async Task OnNavigatedFrom()
        {
            await _viewModel.StopTimer();
        }
    }
}
