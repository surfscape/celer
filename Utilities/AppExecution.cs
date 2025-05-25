using System.Diagnostics;
using System.Windows;

namespace Celer.Utilities
{
    public static class AppExecution
    {
    public static void RestartApplication()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}
