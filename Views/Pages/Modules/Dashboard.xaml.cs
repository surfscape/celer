using Celer.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
            Loaded += Dashboard_Loaded;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();

            if (DataContext is DashboardViewModel viewModel && viewModel.IsLoading)
            {
                await viewModel.InitializeAsync();
            }
        }
    }
}
