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
                PingReply reply = ping.Send("9.9.9.9", 2000); // TODO: add a preference to choose a different ping server if the default one is blocked in other countries
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
