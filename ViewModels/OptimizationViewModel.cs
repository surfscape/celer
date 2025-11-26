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
                { "Battery", new NavigationSubView("Power Manager", "Check the state of your computer battery, and change system power plans", batteryView) },
                { "Memory", new NavigationSubView("Memory Manager", "Check, clean, and configure memory behaviour",memoryView) },
                { "Video", new NavigationSubView("Video Manager", "GPU and DWM settings", videoView) },
                { "Sensors", new NavigationSubView("Sensors", "View your system sensors in real-time",sensorsView) },
            };
        }
    }
}
