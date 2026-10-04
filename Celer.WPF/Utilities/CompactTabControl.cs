using Celer.Properties;
using Celer.Views.Pages.Settings;
using CommunityToolkit.Mvvm.Messaging;
using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Utilities
{
	public partial class CompactTabControl : TabControl
	{
		[DependencyProperty]
		public bool CompactMode { get; set; } = MainConfiguration.Default.SidebarCompactMode;

		[DependencyProperty]
		public double UserContentSize { get; set; } = MainConfiguration.Default.ViewFillContent ? double.PositiveInfinity : 980;

		[DependencyProperty]
		public string Scroll { get; set; } = "auto";

		public CompactTabControl()
		{

			WeakReferenceMessenger.Default.Register<ViewportChangedMessage>(this, (r, m) =>
			{
				UserContentSize = m.Value ? double.PositiveInfinity : 980;
			});
		}

	}


}
