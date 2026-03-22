using System.Diagnostics;
using System.Windows;

namespace Celer.Utilities
{
    public class Processes
    {
        /*public static void KillExplorer()
        {
            var explorers = Process.GetProcessesByName("explorer");
            foreach (var proc in explorers)
            {
                try
                {
                    proc.Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro ao finalizar o processo Explorer: {ex.Message}",
                        "Celer Processes",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }*/

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
                Process.Start("explorer.exe");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start explorer.exe {ex.Message}");
            }

        }
    }
}
