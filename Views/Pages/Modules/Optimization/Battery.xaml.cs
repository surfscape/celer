using Celer.ViewModels;
using Celer.ViewModels.OptimizationVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules.Optimization
{
    /// <summary>
    /// Interaction logic for Battery.xaml
    /// </summary>
    public partial class Battery : UserControl
    {        
        public Battery()
        {
            InitializeComponent();
            Loaded += Battery_Loaded;
        }

        private async void Battery_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (DataContext is BatteryViewModel viewModel && viewModel.IsLoading)
            {
                await viewModel.Initialize();
            }
        }
    }
}
