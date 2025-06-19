using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Celer.ViewModels.ManutencaoVM
{
    public partial class RepairViewModel : ObservableObject
    {
        public ObservableCollection<RepairStep> RepairSteps { get; } =
            [
                new RepairStep(
                    "DISM",
                    "Repara e verifica a integridade dos ficheiros do sistema.",
                    "dism /online /cleanup-image /restorehealth"
                ),
                new RepairStep(
                    "SFC",
                    "Verifica e repara ficheiros corrompidos do sistema.",
                    "sfc /scannow"
                ),
                new RepairStep(
                    "CheckDisk",
                    "Verificar e corrigir erros no sistema de ficheiros e setores no disco.",
                    "chkdsk C: /f /r",
                    requiresReboot: true
                ),
            ];

        [ObservableProperty]
        private double overallProgress;

        [ObservableProperty]
        private bool allSelected = true;

        [ObservableProperty]
        private bool isLoading = true;

        public RepairViewModel()
        {
            foreach (var step in RepairSteps)
            {
                step.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(step.IsSelected))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StartRepairCommand.NotifyCanExecuteChanged();
                        });
                    }
                };
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartRepair))]
        private async Task StartRepairAsync()
        {
            OverallProgress = 0;
            var selectedSteps = RepairSteps.Where(s => s.IsSelected).ToList();
            int stepCount = selectedSteps.Count;

            for (int i = 0; i < stepCount; i++)
            {
                var step = selectedSteps[i];
                step.Progress = 0;

                if (step.RequiresReboot)
                {
                    step.StatusMessage = "Agendado para próximo arranque";
                    step.Progress = 100;
                }
                else
                {
                    await ExecuteCommandWithProgressAsync(step);
                }

                OverallProgress = ((i + 1) / (double)stepCount) * 100;
            }
        }

        private bool CanStartRepair() => RepairSteps.Any(s => s.IsSelected);

        [RelayCommand]
        private void ToggleSelectAll()
        {
            AllSelected = !AllSelected;
            foreach (var step in RepairSteps)
                step.IsSelected = AllSelected;

            StartRepairCommand.NotifyCanExecuteChanged();
        }

        private static async Task ExecuteCommandWithProgressAsync(RepairStep step)
        {
            IProgress<int> progress = new Progress<int>(value => step.Progress = value);
            IProgress<string> status = new Progress<string>(message =>
                step.StatusMessage = message
            );

            try
            {
                status.Report("Em execução...");

                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {step.Command}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    using var process = new Process { StartInfo = psi };

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            status.Report(args.Data);

                            var match = Regex.Match(
                                args.Data,
                                @"(\d{1,3})%\s+complete",
                                RegexOptions.IgnoreCase
                            );
                            if (
                                match.Success
                                && int.TryParse(match.Groups[1].Value, out int percent)
                            )
                            {
                                progress.Report(percent);
                            }
                        }
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            status.Report(args.Data);

                            var match = Regex.Match(
                                args.Data,
                                @"(\d{1,3})%\s+complete",
                                RegexOptions.IgnoreCase
                            );
                            if (
                                match.Success
                                && int.TryParse(match.Groups[1].Value, out int percent)
                            )
                            {
                                progress.Report(percent);
                            }
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    progress.Report(100);
                    status.Report("Concluído.");
                });
            }
            catch (Exception ex)
            {
                status.Report($"Erro: {ex.Message}");
                progress.Report(100);
            }
        }

        public partial class RepairStep(
            string name,
            string description,
            string command,
            bool requiresReboot = false
        ) : ObservableObject
        {
            public string Name { get; } = name;
            public string Description { get; } = description;
            public string Command { get; } = command;
            public bool RequiresReboot { get; } = requiresReboot;

            [ObservableProperty]
            private double progress;

            [ObservableProperty]
            private string statusMessage = "Em espera";

            [ObservableProperty]
            private bool isSelected = true;
        }
    }
}
