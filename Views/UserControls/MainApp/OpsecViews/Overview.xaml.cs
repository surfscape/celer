using Celer.ViewModels.OpsecVM;
using System.Windows.Controls;

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
