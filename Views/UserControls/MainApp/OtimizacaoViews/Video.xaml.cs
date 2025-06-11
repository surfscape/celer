using Celer.ViewModels.OtimizacaoVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp.OtimizacaoViews
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public partial class Video : UserControl
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

    }
}
