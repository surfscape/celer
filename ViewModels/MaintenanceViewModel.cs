using Celer.Models;
using Celer.Services;
using Celer.Views.UserControls.MainApp.MaintenanceViews;

namespace Celer.ViewModels
{
    public partial class MaintenanceViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public MaintenanceViewModel(

            NavigationService navigationService,
            Repair repairView,
            Network networkView
        )
            : base(navigationService, "Maintenance")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Repair", new NavigationSubView("System Repair", "Run the official system repair utilities to repair erros in the disk/image",repairView) },
                { "Network", new NavigationSubView("Network Test", "Check your network connection and change it's DNS", networkView) },
            };
        }
    }
}
