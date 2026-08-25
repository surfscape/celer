using Celer.Infrastructure.Models.Windows;
using Celer.Infrastructure.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Celer.ViewModels.OptimizationVM
{
	public partial class SchedulingViewModel : ObservableObject
	{
		private readonly IScheduler _scheduler;

		[ObservableProperty]
		public partial ObservableCollection<WinPriorityOption> PriorityOptions { get; set; }

		[ObservableProperty]
		public partial WinPriorityOption? SelectedOption { get; set; }

		[ObservableProperty]
		public partial bool IsDefaultValue { get; set; } = true;

		public SchedulingViewModel(IScheduler cpuScheduler)
		{
			_scheduler = cpuScheduler;

			var options = _scheduler.PriorityOptions
				.Select(kvp => new WinPriorityOption(kvp.Key, kvp.Value))
				.ToList();

			PriorityOptions = new ObservableCollection<WinPriorityOption>(options);

			int currentMask = _scheduler.GetPrioritySeparation();
			SelectedOption = PriorityOptions.FirstOrDefault(o => o.MaskValue == currentMask)
							 ?? PriorityOptions.FirstOrDefault();
		}

		[RelayCommand]
		private void ResetValueToDefault()
		{
			SelectedOption = PriorityOptions.FirstOrDefault(o => o.MaskValue == 2);
		}

		partial void OnSelectedOptionChanged(WinPriorityOption? value)
		{
			if (value == null) return;

			IsDefaultValue = value.MaskValue == 2 || value.MaskValue == 38;

			_scheduler.SetPrioritySeparation(value.MaskValue);
		}

	}
}
