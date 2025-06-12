using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Celer.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Celer.ViewModels
{
    public partial class CleanEngine : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CleanupCategory> categories = [];

        [ObservableProperty]
        private double totalFreedText = 0;

        [ObservableProperty]
        private object? selectedItem;

        [ObservableProperty]
        private bool canClean;

        public class LogBook()
        {
            public string? LogEntry { get; set; }
            public SolidColorBrush LogColor { get; set; } = new SolidColorBrush(Colors.Gray);
        }

        public ObservableCollection<LogBook> LogEntries { get; } = [];

        public CleanEngine()
        {
            AppGlobals.EnableCleanEngineChanged += AppGlobals_EnableCleanEngineChanged;
        }

        private void AppGlobals_EnableCleanEngineChanged(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CanClean = AppGlobals.EnableCleanEngine;
            });
        }

        partial void OnCanCleanChanged(bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
            {
                LoadJson();
            }
        }

        private void AddLog(string message, Color foreground)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(
                    new LogBook { LogEntry = message, LogColor = new SolidColorBrush(foreground) }
                );
            });
        }

        private void LoadJson()
        {
            Categories.Clear();
            const string path = "signatures.json";
            if (!File.Exists(path))
            {
                AddLog(
                    "Ficheiro de assinaturas de limpeza não encontrado, tenta atualizar as assinaturas através do menu > Ferramentas > Verificar Atualizações",
                    Colors.Red
                );
                AppGlobals.EnableCleanEngine = false;
                return;
            }
            try
            {
                AddLog(
                    "A carregar assinaturas...",
                    (Color)Application.Current.Resources["SteelError"]
                );
                var json = File.ReadAllText(path);
                ParseJson(json);
                AddLog("Assinaturas carregadas com sucesso!", Colors.YellowGreen);
            }
            catch (Exception e)
            {
                AddLog($"Ocorreu um erro a carregar as assinaturas: {e.Message}", Colors.Red);
                AppGlobals.EnableCleanEngine = false;
            }
        }

        private void ParseJson(string json)
        {
            var doc = JsonDocument.Parse(json);
            foreach (var cat in doc.RootElement.EnumerateObject())
            {
                var items = new ObservableCollection<CleanupItem>();
                foreach (var item in cat.Value.EnumerateArray())
                {
                    var name = item.GetProperty("name").GetString();
                    var path = item.GetProperty("path").GetString();

                    if (name != null && path != null)
                    {
                        items.Add(
                            new CleanupItem
                            {
                                Name = name,
                                Path = path,
                                RequiredProcesses =
                                [
                                    .. item.GetProperty("requiredProcesses")
                                        .EnumerateArray()
                                        .Select(p => p.GetString()!),
                                ],
                                IsChecked = false,
                            }
                        );
                    }
                }
                Categories.Add(new CleanupCategory { Name = cat.Name, Items = items });
            }
        }

        [RelayCommand]
        private async Task CleanAsync()
        {
            TotalFreedText = 0;

            var selectedItems = Categories
                .SelectMany(c => c.Items)
                .Where(i => i.IsChecked)
                .ToList();

            if (selectedItems.Count == 0)
            {
                AddLog(
                    "É necessário selecionar pelo menos um item para iniciar a limpeza.",
                    Colors.Orange
                );
                AppGlobals.EnableCleanEngine = true;
                return;
            }

            var runningProcs = Process
                .GetProcesses()
                .Select(p => p.ProcessName.ToLower())
                .ToHashSet();

            var toClose = new HashSet<string>();

            foreach (var item in selectedItems)
            {
                foreach (var proc in item.RequiredProcesses)
                {
                    if (runningProcs.Contains(Path.GetFileNameWithoutExtension(proc).ToLower()))
                        toClose.Add(proc);
                }
            }

            if (toClose.Count > 0)
            {
                AddLog(
                    "É necessário fechar as seguintes aplicações para continuar:\n"
                        + string.Join("\n", toClose),
                    Colors.Red
                );
                return;
            }

            StringBuilder log = new();
            long totalFreed = 0;

            await Task.Run(() =>
            {
                AppGlobals.EnableCleanEngine = false;
                AddLog(
                    "A iniciar Celer Cleaning Engine...",
                    (Color)Application.Current.Resources["SteelError"]
                );

                foreach (var item in selectedItems)
                {
                    string resolvedPath = Environment.ExpandEnvironmentVariables(item.Path);
                    long freed = 0;

                    try
                    {
                        if (Directory.Exists(resolvedPath))
                        {
                            var dir = new DirectoryInfo(resolvedPath);
                            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                            {
                                try
                                {
                                    freed += file.Length;
                                    file.Attributes = FileAttributes.Normal;
                                    file.Delete();
                                    AddLog(
                                        $"Removido o ficheiro: {file.FullName}",
                                        Colors.YellowGreen
                                    );
                                }
                                catch (Exception ex)
                                {
                                    AddLog(
                                        $"Falha ao apagar ficheiro: {file.FullName}: {ex.Message}",
                                        (Color)Application.Current.Resources["SteelError"]
                                    );
                                }
                            }
                            try
                            {
                                dir.Delete(true);
                            }
                            catch (Exception ex)
                            {
                                AddLog(
                                    $"Falha ao apagar pasta: {resolvedPath}: {ex.Message}",
                                    (Color)Application.Current.Resources["SteelError"]
                                );
                            }
                        }
                        else if (File.Exists(resolvedPath))
                        {
                            var fileInfo = new FileInfo(resolvedPath)
                            {
                                Attributes = FileAttributes.Normal,
                            };
                            freed += fileInfo.Length;
                            fileInfo.Delete();
                            AddLog($"Removido o ficheiro: {fileInfo.FullName}", Colors.YellowGreen);
                        }
                        AddLog($"Removido {resolvedPath} ({freed / 1024} KB)", Colors.YellowGreen);
                    }
                    catch (Exception ex)
                    {
                        AddLog(
                            $"Falha ao apagar {resolvedPath}: {ex.Message}",
                            (Color)Application.Current.Resources["SteelError"]
                        );
                    }

                    Interlocked.Add(ref totalFreed, freed);
                }
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (
                    var line in log.ToString()
                        .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                )
                {
                    LogEntries.Add(
                        new LogBook
                        {
                            LogEntry = line,
                            LogColor = new SolidColorBrush(Colors.Green),
                        }
                    );
                }
            });

            TotalFreedText = (long)(totalFreed / 1024f / 1024f);
            AppGlobals.EnableCleanEngine = true;
        }

        public partial class CleanupItem : ObservableObject
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public List<string>? RequiredProcesses { get; set; }

            [ObservableProperty]
            private bool isChecked;
        }

        public class CleanupCategory
        {
            public string Name { get; set; }
            public ObservableCollection<CleanupItem> Items { get; set; }
        }
    }
}
