using Celer.Models;
using Celer.Services;
using Celer.ViewModels.MaintenanceVM;
using Celer.Views.Pages.Modules.Maintenance;

namespace Celer.ViewModels
{
    public partial class MaintenanceViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, SubviewDescriptor> _views;

        protected override Dictionary<string, SubviewDescriptor> SubViews => _views;

        public MaintenanceViewModel(
            NavigationService navigationService,
            IServiceProvider serviceProvider
        )
            : base(navigationService, NavigationTabKey.Maintenance, serviceProvider)
        {
            _views = new Dictionary<string, SubviewDescriptor>
            {
                { "Repair", new SubviewDescriptor { Id = "Repair", Name = "System Repair", Description = "Run the official system repair utilities to check & repair erros in the disk/image", ViewModelType = typeof(RepairViewModel) } },
                { "Network", new SubviewDescriptor { Id = "Network", Name = "Network Manager", Description = "Check your network connection and change DNS system-wide", ViewModelType = typeof(NetworkViewModel) } },
            };
        }
    }
}
