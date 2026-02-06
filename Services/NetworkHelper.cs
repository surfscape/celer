using System.Diagnostics;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace Celer.Services
{
    public static class NetworkHelper
    {
        public static async Task<bool> HasNetworkAdapters()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Any(ni => ni.OperationalStatus == OperationalStatus.Up);
        }

        public static async Task<bool> IsConnected()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public static async Task<bool> HasInternetAccess()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var result = await client.GetAsync("https://www.google.com");
                return result.IsSuccessStatusCode;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Failed to get internet access status: {ex.Message}");
                return false;
            }
        }

        public static async Task<string> PingAsync(string host)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(host, 1000);
                return reply.Status == IPStatus.Success ? reply.RoundtripTime.ToString() : "Timeout";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get internet acess status: {ex.Message}");
                return "Failed";
            }
        }
        public static async Task<bool> SetSystemDnsAsync(string dns)
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni =>
                        ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                foreach (var ni in interfaces)
                {
                    string name = ni.Name;


                    string command = $"Set-DnsClientServerAddress -InterfaceAlias \"{name}\" -ServerAddresses \"{dns}\"";

                    ProcessStartInfo psi = new()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"{command}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    };

                    using var process = Process.Start(psi);
                    if (process is not null)
                    await process.WaitForExitAsync();
                }

                return true;
            }
            catch(NetworkInformationException ex)
            {
                Debug.WriteLine($"Faild to retrieve all network interfaces\n {ex.Message}");
                return false;
            }
            catch(ArgumentNullException ex)
            {
                Debug.WriteLine($"Faild to filter network interfaces\n {ex.Message}");
                return false;
            }
            catch(InvalidOperationException ex)
            {
                Debug.WriteLine($"Faild to start the process\n {ex.Message}");
                return false;
            }
            catch(Win32Exception ex)
            {
                Debug.WriteLine($"Faild to start the process\n {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Faild to start the process\n {ex.Message}");
                return false;
            }
        }
    }

}
