namespace Celer.Infrastructure.Windows
{
	public interface IScheduler
	{
		IReadOnlyDictionary<int, string> PriorityOptions { get; }

		int GetPrioritySeparation();

		void SetPrioritySeparation(int maskValue);
	}
}
