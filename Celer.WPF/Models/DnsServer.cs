using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models
{
	public partial class DnsServer(string name, string ipv4primary, string ipv4secondary) : ObservableObject
	{
		public string Name { get; } = name;
		public string Ipv4Primary { get; } = ipv4primary;
		public string Ipv4Secondary { get; } = ipv4secondary;

		[ObservableProperty]
		public partial string PingStatus { get; set; } = "N/A";
	}
}
