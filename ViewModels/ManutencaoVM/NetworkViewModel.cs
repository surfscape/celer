using Celer.Models;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace Celer.ViewModels.ManutencaoVM
{
    public partial class NetworkViewModel : ObservableObject
    {
        public ObservableCollection<DnsServer> DnsServers { get; } = new();


        [ObservableProperty]
        private DnsServer? selectedDnsServer;

        public string NetworkAdaptersStatus { get; set; } = "Desconhecido";
        public string ConnectionStatus { get; set; } = "Desconhecido";
        public string InternetStatus { get; set; } = "Desconhecido";


        public NetworkViewModel()
        {
            LoadDnsServers();
            UpdatePing();
        }

        [RelayCommand]
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
        private async void TestNetwork()
        {
            NetworkAdaptersStatus = await NetworkHelper.HasNetworkAdapters() ? "Encontrado(s)" : "Nenhum";
            ConnectionStatus = await NetworkHelper.IsConnected() ? "Ativa" : "Sem Conexão";
            InternetStatus = await NetworkHelper.HasInternetAccess() ? "Com Acesso" : "Sem Internet";
            OnPropertyChanged(nameof(NetworkAdaptersStatus));
            OnPropertyChanged(nameof(ConnectionStatus));
            OnPropertyChanged(nameof(InternetStatus));
        }

        [RelayCommand]
        private async void UpdatePing()
        {
            foreach (var dns in DnsServers)
            {
                dns.Ping = await NetworkHelper.PingAsync(dns.IP);
            }
        }

        [RelayCommand]
        private async void SetDns()
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
