using Celer.ViewModels.OptimizationVM;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules.Optimization
{
    /// <summary>
    /// Interaction logic for MemoryManagement.xaml
    /// </summary>
    public partial class MemoryManagement : UserControl
    {
        public MemoryManagement()
        {
            InitializeComponent();
            Loaded += Memory_Loaded;
        }
        private async void Memory_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            if (DataContext is MemoryViewModel viewModel && viewModel.IsLoading)
            {
                await viewModel.Initialize();
            }
        }

    }
}
