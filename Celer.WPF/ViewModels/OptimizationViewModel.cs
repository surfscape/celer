using Celer.Models;
using Celer.Services;
using Celer.ViewModels.OptimizationVM;

namespace Celer.ViewModels
{
	public partial class OptimizationViewModel : BaseNavigationViewModel
	{
		private readonly Dictionary<string, SubviewDescriptor> _views;

		protected override Dictionary<string, SubviewDescriptor> SubViews => _views;

		public OptimizationViewModel(
			NavigationService navigationService,
			IServiceProvider serviceProvider
		)
		: base(navigationService, NavigationTabKey.Optimization, serviceProvider)
		{
			_views = new Dictionary<string, SubviewDescriptor>
			{
				{ "Battery", new SubviewDescriptor { Id = "Battery", Name = "Power Manager", Description = "Check the state of your computer battery, and change system power plans", ViewModelType = typeof(BatteryViewModel) } },
				{ "Memory", new SubviewDescriptor { Id = "Memory", Name = "Memory Manager", Description = "Check, clean, and configure memory behaviour", ViewModelType = typeof(MemoryViewModel) } },
				{ "Video", new SubviewDescriptor { Id = "Video", Name = "Video Manager", Description = "GPU and DWM settings", ViewModelType = typeof(VideoViewModel) } },
				{ "Sensors", new SubviewDescriptor { Id = "Sensors", Name = "Sensors", Description = "View your system sensors in real-time", ViewModelType = typeof(SensorViewModel) } },
				{ "Scheduling", new SubviewDescriptor { Id = "Scheduling", Name = "Process Scheduling", Description = "Change how Windows handles foreground and background priority and boosting", ViewModelType = typeof(SchedulingViewModel) } },
			};
		}
	}
}