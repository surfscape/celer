using Celer.Interfaces;
using Celer.ViewModels.OptimizationVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules.Optimization
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : UserControl, INavigationAware
    {
        public Video()
        {
            InitializeComponent();
            Loaded += Video_Loaded;
        }

        private async void Video_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (DataContext is VideoViewModel viewModel && viewModel.IsLoading)
            {
                await viewModel.Initialize();
            }
        }

        public async Task OnNavigatedTo()
        {
            if (DataContext is VideoViewModel viewModel && !viewModel.IsLoading)
            {
                await viewModel.StartTimerAsync();
            }
        }

        public async Task OnNavigatedFrom()
        {
            if(DataContext is VideoViewModel viewModel)
            await viewModel.StopTimerAsync();
        }
    }
}
