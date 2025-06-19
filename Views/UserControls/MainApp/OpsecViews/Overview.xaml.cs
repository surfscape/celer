using System.Windows.Controls;
using Celer.ViewModels.OpsecVM;

namespace Celer.Views.UserControls.MainApp.OpsecViews
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public Overview()
        {
            InitializeComponent();
            DataContext = new OverviewViewModel();
        }
    }
}
