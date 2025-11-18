using Celer.Models;
using Celer.Services;
using Celer.Views.UserControls.MainApp.OptimizationViews;

namespace Celer.ViewModels
{
    public partial class OptimizationViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public OptimizationViewModel(
            NavigationService navigationService,
            MemoryManagement memoryView,
            Sensors sensorsView,
            Battery batteryView,
            Video videoView
        )
            : base(navigationService, "Optimization")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Memory", new NavigationSubView("Memory Manager", memoryView) },
                { "Battery", new NavigationSubView("Battery Manager", batteryView) },
                { "Video", new NavigationSubView("Video Manager", videoView) },
                { "Sensors", new NavigationSubView("Sensors", sensorsView) },
            };
        }
    }
}
