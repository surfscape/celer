using Celer.Models;
using Celer.Services;
using Celer.Views.UserControls.MainApp.SubOtimização;

namespace Celer.ViewModels
{
    public partial class OtimizacaoViewModel : BaseNavigationViewModel
    {

        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public OtimizacaoViewModel(NavigationService navigationService, MemoryManagement memoryView, Sensors sensors)
            : base(navigationService, "Otimizacao")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Memory", new NavigationSubView("Gestão de Memória", memoryView) },
                { "Sensors", new NavigationSubView("Sensores", sensors) }
            };
        }
    }

}
