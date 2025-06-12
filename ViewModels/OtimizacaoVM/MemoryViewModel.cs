using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Celer.Models.SystemInfo;
using Celer.Services.Memory;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;

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
            ShowTaskDialog();
        }

        private void ShowTaskDialog()
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
                TaskDialogButton okButton = new(ButtonType.Ok);
                TaskDialogButton cancelButton = new(ButtonType.Cancel);
                dialog.Buttons.Add(okButton);
                dialog.Buttons.Add(cancelButton);
                dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>(
                    TaskDialog_HyperLinkClicked
                );
                TaskDialogButton button = dialog.ShowDialog();
                if (button == okButton)
                    MessageBox.Show("You clicked the OK button.", "Task Dialog Sample");
            }
            else
            {
                MessageBox.Show(
                    "O sistema não suporta TaskDialog, verifique se está a executar o Celer no Windows 10/11 e que tem a última versão de .NET 9 instalada",
                    "TaskDialog Failure"
                );
            }
        }

        private void TaskDialog_HyperLinkClicked(object? sender, HyperlinkClickedEventArgs e)
        {
            if (e.Href != null)
            {
                Process.Start(new ProcessStartInfo { FileName = e.Href, UseShellExecute = true });
            }
        }

        [RelayCommand]
        private void CompressMemory()
        {
            // Apenas possível em Windows 10+ com comandos internos
        }

        [RelayCommand]
        private void EditPagefile()
        {
            // Abrir painel ou usar código WMI para editar pagefile
        }
    }
}
