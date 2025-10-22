using Celer.Resources.i18n;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Celer.Views.Windows.Utils
{
    public enum CheckStatus
    {
        Pending,
        Success,
        Failure
    }
    public partial class CheckItem : ObservableObject
    {

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private string message = Strings.WaitMessage;

        [ObservableProperty]
        private CheckStatus status = CheckStatus.Pending;
    }

    public partial class AmbientChecker : Window
    {
        public ObservableCollection<CheckItem> Checks { get; set; }

        public AmbientChecker()
        {
            InitializeComponent();

            Checks = new ObservableCollection<CheckItem>
            {
                new() { Name = "Microsoft Edge" },
                new() { Name = "DISM" },
                new() { Name = "SFC" },
                new() { Name = "CHKDSK" },
                new() { Name = "Winget" },
                new() { Name = "Microsoft Store" }
            };

            ChecksList.ItemsSource = Checks;
        }

        private async void StartCheck_Click(object sender, RoutedEventArgs e)
        {
            StartCheckButton.IsEnabled = false;

            await PerformCheck(Checks[0], CheckEdgeInstalledAsync);
            await PerformCheck(Checks[1], () => CheckCommandExistsAsync("dism.exe"));
            await PerformCheck(Checks[2], () => CheckCommandExistsAsync("sfc.exe"));
            await PerformCheck(Checks[3], () => CheckCommandExistsAsync("chkdsk.exe"));
            await PerformCheck(Checks[4], () => CheckCommandExistsAsync("winget.exe"));
            await PerformCheck(Checks[5], CheckStoreInstalledAsync);

            StartCheckButton.Content = Strings.AmbientChecker_DoneLabel;
        }

        private async Task PerformCheck(CheckItem item, Func<Task<(bool, string)>> checkFunction)
        {
            var (success, message) = await checkFunction();
            item.Status = success ? CheckStatus.Success : CheckStatus.Failure;
            item.Message = message;
        }

        private async Task<(bool, string)> CheckEdgeInstalledAsync()
        {
            return await Task.Run(() =>
            {
                string[] registryPaths = {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (var path in registryPaths)
                {
                    using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(path))
                    {
                        if (key != null)
                        {
                            foreach (string subkeyName in key.GetSubKeyNames())
                            {
                                using (RegistryKey? subkey = key.OpenSubKey(subkeyName))
                                {
                                    if (subkey?.GetValue("DisplayName")?.ToString() == "Microsoft Edge")
                                    {
                                        return (true, $"Instalado (Versão: {subkey.GetValue("DisplayVersion")})");
                                    }
                                }
                            }
                        }
                    }
                }
                return (false, Strings.NotFound);
            });
        }

        private async Task<(bool, string)> CheckCommandExistsAsync(string command)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null) return (false, "Falha ao iniciar o processo.");

                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    return (true, Strings.Available);
                }
                return (false, Strings.NotFound);
            }
        }
        private async Task<(bool, string)> CheckStoreInstalledAsync()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"Get-AppxPackage *Microsoft.WindowsStore*\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null) return (false, "Falha ao iniciar o PowerShell.");

                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    return (true, Strings.Installed);
                }
                return (false, Strings.NotFound);
            }
        }
    }
}
