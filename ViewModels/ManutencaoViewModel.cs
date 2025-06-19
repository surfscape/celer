using Celer.Models;
using Celer.Services;
using Celer.Views.UserControls.MainApp.ManutencaoViews;

namespace Celer.ViewModels
{
    public partial class ManutencaoViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public ManutencaoViewModel(
            NavigationService navigationService,
            Repair repairView,
            Realtek realtekView,
            Network networkView
        )
            : base(navigationService, "Manutencao")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Repair", new NavigationSubView("Recuperação", repairView) },
                { "Realtek", new NavigationSubView("Realtek Audio Wizard", realtekView) },
                { "Network", new NavigationSubView("Teste de Internet", networkView) },
            };
        }
    }
}
