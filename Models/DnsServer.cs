using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models
{
    public class DnsServer : ObservableObject
    {
        public string Name { get; }
        public string IP { get; }

        private string _ping = "N/A";
        public string Ping
        {
            get => _ping;
            set => SetProperty(ref _ping, value);
        }

        public DnsServer(string name, string ip)
        {
            Name = name;
            IP = ip;
        }
    }
}
