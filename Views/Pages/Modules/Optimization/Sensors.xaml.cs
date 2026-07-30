using Celer.Interfaces;
using Celer.ViewModels.OptimizationVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules.Optimization
{
    /// <summary>
    /// Interaction logic for Sensors.xaml
    /// </summary>
    public partial class Sensors : UserControl, INavigationAware
    {
        public Sensors()
        {
            InitializeComponent();
            Loaded += Sensors_Loaded;
        }

        private async void Sensors_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (DataContext is SensorViewModel viewModel && viewModel.IsLoading)
            {
                await viewModel.Initialize();
            }
        }

        public async Task OnNavigatedTo()
        {
            if (DataContext is SensorViewModel viewModel && !viewModel.IsLoading)
            {
                await viewModel.StartTimer();
            }
        }

        public async Task OnNavigatedFrom()
        {
            if (DataContext is SensorViewModel viewModel)
                await viewModel.StopTimer();
        }
    }
}
