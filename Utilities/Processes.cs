using System.Diagnostics;

namespace Celer.Utilities
{
    public class Processes
    {

        public static void KillExplorer()
        {
            var startInfo = new ProcessStartInfo("cmd", $"/c taskkill /F /IM explorer.exe")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };
            using var process = Process.Start(startInfo);
        }

        public static void StartExplorer()
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe")
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start explorer.exe {ex.Message}");
            }
        }
    }
}
