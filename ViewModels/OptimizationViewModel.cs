using Celer.Models;
using Celer.Services;
using Celer.ViewModels.OptimizationVM;

namespace Celer.ViewModels
{
    public partial class OptimizationViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public OptimizationViewModel(
            NavigationService navigationService,
            MemoryViewModel memoryViewModel,
            SensorViewModel sensorsViewModel,
            BatteryViewModel batteryViewModel,
            VideoViewModel videoViewModel
        )
            : base(navigationService, "Optimization")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Battery", new NavigationSubView("Power Manager", "Check the state of your computer battery, and change system power plans", batteryViewModel) },
                { "Memory", new NavigationSubView("Memory Manager", "Check, clean, and configure memory behaviour", memoryViewModel) },
                { "Video", new NavigationSubView("Video Manager", "GPU and DWM settings", videoViewModel) },
                { "Sensors", new NavigationSubView("Sensors", "View your system sensors in real-time", sensorsViewModel) },
            };
        }
    }
}