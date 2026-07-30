using Celer.Models;
using Celer.Services;
using Celer.ViewModels.MaintenanceVM;
using Celer.Views.Pages.Modules.Maintenance;

namespace Celer.ViewModels
{
    public partial class MaintenanceViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public MaintenanceViewModel(
            NavigationService navigationService,
            RepairViewModel repairViewModel,
            NetworkViewModel networkViewModel
        )
            : base(navigationService, "Maintenance")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Repair", new NavigationSubView("System Repair", "Run the official system repair utilities to check & repair erros in the disk/image",repairViewModel) },
                { "Network", new NavigationSubView("Network Manager", "Check your network connection and change DNS system-wide", networkViewModel) },
            };
        }
    }
}
