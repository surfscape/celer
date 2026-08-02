using Celer.Models;
using Celer.Services;
using Celer.ViewModels.SoftwareVM;

namespace Celer.ViewModels
{
	public class SoftwareViewModel : BaseNavigationViewModel
	{

		private readonly Dictionary<string, SubviewDescriptor> _views;

		protected override Dictionary<string, SubviewDescriptor> SubViews => _views;

		public SoftwareViewModel(
			NavigationService navigationService,
			IServiceProvider serviceProvider
		)
		: base(navigationService, NavigationTabKey.Software, serviceProvider)
		{
			_views = new Dictionary<string, SubviewDescriptor>
			{
				{ "PackageManager", new SubviewDescriptor { Id = "PackageManager", Name = "Package Manager", Description = "Install or upgrade software", ViewModelType = typeof(PackageManagerViewModel) } },
			};
		}
	}
}
