using Celer.Interfaces;
using Celer.Models;
using Celer.Services;
using Celer.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Celer.ViewModels.MaintenanceVM
{
	public partial class NetworkViewModel : ObservableObject, INavigationAware
	{
		public ObservableCollection<DnsServer> DnsServers { get; set; } = [];

		[ObservableProperty]
		public partial DnsServer? SelectedDnsServer { get; set; }

		[ObservableProperty]
		public partial string AdaptersFound { get; set; } = "N/A";

		[ObservableProperty]
		public partial string ConnectionStatus { get; set; } = "N/A";

		[ObservableProperty]
		public partial string InternetConnectionStatus { get; set; } = "N/A";

		private void LoadDnsServers()
		{
			DnsServers.Clear();
			DnsServers.Add(new DnsServer("Cloudflare", "1.1.1.1", "1.0.0.1"));
			DnsServers.Add(new DnsServer("Google", "8.8.8.8", "8.8.4.4"));
			DnsServers.Add(new DnsServer("AdGuard DNS", "94.14.14.14", "94.14.15.15"));
			DnsServers.Add(new DnsServer("Quad9", "9.9.9.9", "149.112.112.112"));
			DnsServers.Add(new DnsServer("NextDNS", "45.90.28.0", "45.90.30.0"));
			DnsServers.Add(new DnsServer("DNS4EU", "86.54.11.1", "86.54.11.201"));
		}

		[RelayCommand]
		private async Task TestNetwork()
		{
			AdaptersFound = NetworkHelper.HasNetworkAdapters() ? "Found" : "None";
			ConnectionStatus = NetworkHelper.IsConnected() ? "Active" : "Unavailable";
			InternetConnectionStatus = await NetworkHelper.HasInternetAccess() ? "Yes" : "Unavailable";
		}

		[RelayCommand]
		public async Task UpdatePing()
		{
			foreach (var dns in DnsServers)
			{
				dns.PingStatus = await NetworkHelper.PingAsync(dns.Ipv4Primary);
			}
			OnPropertyChanged(nameof(DnsServer));
		}

		[RelayCommand]
		private async Task SetDns()
		{
			if (SelectedDnsServer == null)
				return;

			var dnsResult = await NetworkHelper.SetSystemDnsAsync(SelectedDnsServer.Ipv4Primary, SelectedDnsServer.Ipv4Secondary);

			if (dnsResult.Item1)
			{
				Dialog.Show($"The DNS was changed sucessfuly to {SelectedDnsServer.Name}", "Celer DNS Manager", ["OK"], MessageBoxButtons.OKCancel);
			}
			else
			{
				Dialog.Show($"Failed to change DNS\n{dnsResult.Item2}",
								"Celer DNS Manager", ["OK"]);
			}
		}

		public async Task OnNavigatedTo()
		{
			await TestNetwork();
			LoadDnsServers();
			await UpdatePing();
		}

		public async Task OnNavigatedFrom()
		{
			DnsServers.Clear();
		}

	}
}
