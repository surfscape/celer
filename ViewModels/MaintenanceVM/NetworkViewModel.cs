using Celer.Models;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Celer.ViewModels.MaintenanceVM
{
    public partial class NetworkViewModel : ObservableObject
    {
        public ObservableCollection<DnsServer> DnsServers { get; set; } = [];

        [ObservableProperty]
        private DnsServer? selectedDnsServer;

        [ObservableProperty]
        private string adaptersFound = "N/A";

        [ObservableProperty]
        private string connectionStatus  = "N/A";

        [ObservableProperty]
        private string internetConnectionStatus = "N/A";

        public NetworkViewModel()
        {
            LoadDnsServers();
        }

        private void LoadDnsServers()
        {
            DnsServers.Clear();
            DnsServers.Add(new DnsServer("Cloudflare", "1.1.1.1"));
            DnsServers.Add(new DnsServer("Google", "8.8.8.8"));
            DnsServers.Add(new DnsServer("Quad9", "9.9.9.9"));
            DnsServers.Add(new DnsServer("NextDNS", "45.90.28.0"));
            DnsServers.Add(new DnsServer("European DNS", "194.242.2.2"));
        }

        [RelayCommand]
        private async Task TestNetwork()
        {
            AdaptersFound = await NetworkHelper.HasNetworkAdapters() ? "Found" : "None";
            ConnectionStatus = await NetworkHelper.IsConnected() ? "Active" : "No connection";
            InternetConnectionStatus = await NetworkHelper.HasInternetAccess() ? "Access" : "No access";
        }

        [RelayCommand]
        public async Task UpdatePing()
        {
            foreach (var dns in DnsServers)
            {
                dns.PingStatus = await NetworkHelper.PingAsync(dns.IP);
            }
            OnPropertyChanged(nameof(DnsServer));
        }

        [RelayCommand]
        private async Task SetDns()
        {
            if (SelectedDnsServer == null)
                return;

            bool result = await NetworkHelper.SetSystemDnsAsync(SelectedDnsServer.IP);

            if (result)
            {
                MessageBox.Show($"O DNS foi alterado com sucesso para {SelectedDnsServer.Name} ({SelectedDnsServer.IP}).",
                                "DNS Alterado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Falha ao alterar o DNS. Execute a aplicação como Administrador.",
                                "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
