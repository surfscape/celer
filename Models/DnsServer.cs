using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models
{
    public partial class DnsServer(string name, string ip) : ObservableObject
    {
        public string Name { get; } = name;
        public string IP { get; } = ip;

        [ObservableProperty]
        private string pingStatus = "N/A";
    }
}
