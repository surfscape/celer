using Celer.Models.SystemInfo;
using Celer.Services.Memory;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace Celer.ViewModels.OtimizacaoVM
{
    public partial class MemoryViewModel : ObservableObject
    {
        private readonly MemoryMonitorService _monitorService = new();
        private readonly Timer _updateTimer;

        [ObservableProperty]
        private MemoryInfo memory;

        public ObservableCollection<RamSlotInfo> Slots { get; } = new();

        public MemoryViewModel()
        {
            UpdateMemoryInfo(false);
            _updateTimer = new Timer(_ => UpdateMemoryInfo(true), null, 0, 1000);
        }

        private void UpdateMemoryInfo(bool continous)
        {
            var mem = _monitorService.GetMemoryInfo();
            Memory = mem;

            if (!continous)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Slots.Clear();
                    foreach (var slot in mem.Slots)
                        Slots.Add(slot);
                });
            }
        }

        [RelayCommand]
        private void CleanCache()
        {
            ShowCleanCacheDialogAsync();
        }

        private void ShowCleanCacheDialogAsync()
        {
            if (TaskDialog.OSSupportsTaskDialogs)
            {
                using TaskDialog dialog = new();
                dialog.WindowTitle = "Celer Memory Service";
                dialog.MainInstruction = "Limpar cache da memória";
                dialog.Content =
                    "Esta operação irá solicitar ao Windows que execute tarefas pendentes para libertar memória adicional. Durante este processo, poderá ocorrer alguma instabilidade no sistema, pelo que não é recomendado utilizar o computador enquanto a operação decorre.";
                dialog.Footer =
                    "Esta mensagem é apresentada pelo serviço de memória do Celer. Para mais informações sobre o seu funcionamento, consulte: <a href=\"https://surfscape.github.io/blueprint/celer/services/memory\">surfscape.github.io/blueprint/celer/services/memory</a>.";
                dialog.FooterIcon = TaskDialogIcon.Information;
                dialog.EnableHyperlinks = true;

                var okButton = new TaskDialogButton(ButtonType.Ok);
                var cancelButton = new TaskDialogButton(ButtonType.Cancel);
                dialog.Buttons.Add(okButton);
                dialog.Buttons.Add(cancelButton);

                dialog.HyperlinkClicked += TaskDialog_HyperLinkClicked;

                TaskDialogButton result = dialog.ShowDialog();

                if (result == okButton)
                    CleanCacheAction();
            }
            else
            {
                MessageBox.Show(
                    "O sistema não suporta TaskDialog, verifique se está a executar o Celer no Windows 10/11 e que tem a última versão de .NET 9 instalada",
                    "TaskDialog Failure"
                );
            }
        }

        private static void CleanCacheAction()
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "rundll32.exe",
                Arguments = "advapi32.dll,ProcessIdleTasks",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            if (proc is not null)
            {
                proc.Start();
                proc.WaitForExit();
                if (proc.HasExited)
                {
                    MessageBox.Show(
                        "A limpeza do cache foi iniciada. O sistema pode demorar algum tempo a libertar memória adicional. Por favor, aguarde.",
                        "Limpeza de Cache",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
            MessageBox.Show("Erro ao inicar processo", "Limpeza de Cache");
        }

        private void TaskDialog_HyperLinkClicked(object? sender, HyperlinkClickedEventArgs e)
        {
            if (e.Href != null)
            {
                Process.Start(new ProcessStartInfo { FileName = e.Href, UseShellExecute = true });
            }
        }

        [RelayCommand]
        private void EditPagefile()
        {
            Process.Start("SystemPropertiesAdvanced.exe");
        }
    }
}
