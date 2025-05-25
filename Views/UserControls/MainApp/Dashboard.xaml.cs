using System.Windows.Controls;
using Celer.ViewModels;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
