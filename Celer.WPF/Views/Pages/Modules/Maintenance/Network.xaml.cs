using Celer.ViewModels.MaintenanceVM;
using System.Windows;
using System.Windows.Controls;


namespace Celer.Views.Pages.Modules.Maintenance
{
    /// <summary>
    /// Interaction logic for Network.xaml
    /// </summary>
    public partial class Network : UserControl
    {
        
        public Network()
        {
            InitializeComponent();
            Loaded += NetworkLoaded;
        }

        private async void NetworkLoaded(object sender, RoutedEventArgs e)
        {
            if(DataContext is NetworkViewModel viewModel)
            await viewModel.UpdatePing();
        }
    }
}
