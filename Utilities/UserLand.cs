using System.Net.NetworkInformation;

namespace Celer.Utilities
{
    public class UserLand
    {
        public static bool IsInternetAvailable()
        {
            try
            {
                using Ping ping = new();
                PingReply reply = ping.Send("9.9.9.9", 2000); // TODO: add a configuration option for the ping server if the default one is censored in some countries
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
