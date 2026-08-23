using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace Celer.ViewModels.OptimizationVM
{
	public partial class SchedulingViewModel : ObservableObject
	{
		public static readonly Dictionary<int, string> seperationKeys = new()
		{
			{ 2, "Default" },
			{ 42, "Short, Fixed, High foreground boost" },
			{ 41, "Short, Fixed, Medium foreground boost" },
			{ 40, "Short, Fixed, No foreground boost" },
			{ 38, "Short, Variable, High foreground boost" }, // default for applications on the foreground
			{ 37, "Short, Variable, Medium foreground boost" },
			{ 36, "Short, Variable, No foreground boost" },
			{ 26, "Long, Fixed, High foreground boost" },
			{ 25, "Long, Fixed, Medium foreground boost" },
			{ 24, "Long, Fixed, No foreground boost" }, // default for background services
			{ 22, "Long, Variable, High foreground boost" },
			{ 21, "Long, Variable, Medium foreground boost" },
			{ 20, "Long, Variable, No foreground boost" }
		};
		private readonly RegistryKey? currentRegistryValue;

		[ObservableProperty]
		public partial Dictionary<int, string> WinSeperationValues { get; set; } = seperationKeys;

		[ObservableProperty]
		public partial int SelectedWinSeperationIndex { get; set; }

		[ObservableProperty]
		public partial bool IsDefaultValue { get; set; } = true;

		partial void OnSelectedWinSeperationIndexChanged(int value)
		{
			IsDefaultValue = value != 0 && value != 4 ? false : true;
			if (currentRegistryValue != null)
				currentRegistryValue.SetValue("Win32PrioritySeparation", seperationKeys.ElementAt(value).Key);
		}

		public SchedulingViewModel()
		{
			currentRegistryValue = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\PriorityControl\", true);
			int currentValue = 2;
			if (currentRegistryValue != null)
				currentValue = (int)currentRegistryValue.GetValue("Win32PrioritySeparation")!;
			SelectedWinSeperationIndex = seperationKeys.Keys.ToList().IndexOf(currentValue);
			if (currentValue != 2 && currentValue != 38)
				IsDefaultValue = false;
		}

		[RelayCommand]
		private void ResetValueToDefault()
		{
			SelectedWinSeperationIndex = 0;
		}
	}
}
