using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.ViewModels
{
	public partial class BaseModuleViewModel : ObservableObject
	{
		/// <summary>
		/// Property that tracks the current loading progress of the module's view module
		/// </summary>
		[ObservableProperty]
		public partial bool IsLoading { get; set; } = true;
	}
}
