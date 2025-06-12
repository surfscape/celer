using System.Windows;
using System.Windows.Controls;
using Celer.Interfaces;
using Celer.ViewModels.OtimizacaoVM;

namespace Celer.Views.UserControls.MainApp.OtimizacaoViews
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : UserControl, INavigationAware
    {
        private readonly VideoViewModel _viewModel;

        public Video(VideoViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += Video_Loaded;
        }

        private async void Video_Loaded(object sender, RoutedEventArgs e)
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
                await _viewModel.StartTimerAsync();
            }
        }

        public async Task OnNavigatedFrom()
        {
            await _viewModel.StopTimerAsync();
        }
    }
}
