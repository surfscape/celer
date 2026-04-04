using Celer.ViewModels.OptimizationVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp.OptimizationViews
{
    /// <summary>
    /// Interaction logic for MemoryManagement.xaml
    /// </summary>
    public partial class MemoryManagement : UserControl
    {
        private readonly MemoryViewModel _viewModel;
        public MemoryManagement(MemoryViewModel memoryViewModel)
        {
            InitializeComponent();
            _viewModel = memoryViewModel;
            DataContext = _viewModel;
            Loaded += Memory_Loaded;
        }
        private async void Memory_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (_viewModel.IsLoading)
                await _viewModel.Initialize();
        }

    }
}
