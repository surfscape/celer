using System.Diagnostics;
using System.Windows;

namespace Celer.Utilities
{
    public class Processes
    {
        public static void KillExplorer()
        {
            var explorers = Process.GetProcessesByName("explorer");
            foreach (var proc in explorers)
            {
                try
                {
                    proc.Kill();
                    proc.WaitForExit();
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
        }

        public static void StartExplorer()
        {
            try
            {
                Process.Start("explorer.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao reiniciar o processo Explorer: {ex.Message}",
                    "Celer Processes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
