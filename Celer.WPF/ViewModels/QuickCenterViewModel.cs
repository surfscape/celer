using Celer.Infrastructure.Battery;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Threading;

namespace Celer.ViewModels
{
	public partial class QuickCenterViewModel : ObservableObject
	{
		private readonly Battery batteryService;

		private readonly DispatcherTimer _updateTimer = new()
		{
			Interval = TimeSpan.FromSeconds(2),
		};

		[ObservableProperty]
		public partial bool HasBattery { get; set; } = false;

		[ObservableProperty]
		public partial BatteryDevice? CurrentBattery { get; set; }


		public QuickCenterViewModel()
		{
			batteryService = new Battery();
			batteryService.Update();
			if (batteryService.Batteries.Count > 0)
			{
				CurrentBattery = batteryService.Batteries[0];
				HasBattery = true;
			}
			_updateTimer.Tick += (_, _) => UpdateBatteryInfo();
		}

		private void UpdateBatteryInfo()
		{
			if (batteryService is not null)
			{
				batteryService.Update();
				CurrentBattery = batteryService.Batteries[0];
			}
		}
	}
}
