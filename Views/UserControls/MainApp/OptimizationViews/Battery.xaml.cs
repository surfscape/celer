using Celer.ViewModels.OptimizationVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp.OptimizationViews
{
    /// <summary>
    /// Interaction logic for Battery.xaml
    /// </summary>
    public partial class Battery : UserControl
    {
        private readonly BatteryViewModel _viewModel;

        public Battery(BatteryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += Battery_Loaded;
        }

        private async void Battery_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (_viewModel.IsLoading)
            {
                await _viewModel.Initialize();
            }
        }
    }
}
