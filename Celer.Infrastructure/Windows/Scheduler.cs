namespace Celer.Infrastructure.Windows
{
	public class Scheduler : IScheduler
	{
		private const string PriorityControlKeyPath = @"SYSTEM\CurrentControlSet\Control\PriorityControl";
		private const string ValueName = "Win32PrioritySeparation";

		public IReadOnlyDictionary<int, string> PriorityOptions { get; } = new Dictionary<int, string>
		{
			{ 2, "Default" },
			{ 42, "Short, Fixed, High foreground boost" },
			{ 41, "Short, Fixed, Medium foreground boost" },
			{ 40, "Short, Fixed, No foreground boost" },
			{ 38, "Short, Variable, High foreground boost" },
			{ 37, "Short, Variable, Medium foreground boost" },
			{ 36, "Short, Variable, No foreground boost" },
			{ 26, "Long, Fixed, High foreground boost" },
			{ 25, "Long, Fixed, Medium foreground boost" },
			{ 24, "Long, Fixed, No foreground boost" },
			{ 22, "Long, Variable, High foreground boost" },
			{ 21, "Long, Variable, Medium foreground boost" },
			{ 20, "Long, Variable, No foreground boost" }
		};

		public int GetPrioritySeparation()
		{
			using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(PriorityControlKeyPath, false);
			return key?.GetValue(ValueName) is int value ? value : 2;
		}

		public void SetPrioritySeparation(int maskValue)
		{
			using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(PriorityControlKeyPath, true);
			key?.SetValue(ValueName, maskValue, Microsoft.Win32.RegistryValueKind.DWord);
		}
	}
}
