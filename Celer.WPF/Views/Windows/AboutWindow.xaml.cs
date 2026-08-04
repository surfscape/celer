using Celer.Utilities;
using System.Reflection;

namespace Celer.Views.Windows
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : BaseWindow
	{
		private readonly string infoVersion = Assembly
	.GetExecutingAssembly()
	.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
	?.InformationalVersion.Split('+')[0] ?? "Unknown";
		public AboutWindow()
		{
			InitializeComponent();
			Version.Text = infoVersion;
		}
	}
}
