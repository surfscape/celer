using Celer.Models;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

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

        private bool hasRan;

        public class LogBook()
        {
            public string? LogEntry { get; set; }
            public SolidColorBrush LogColor { get; set; } = new SolidColorBrush(Colors.Gray);
        }

        public ObservableCollection<LogBook> LogEntries { get; } = [];

        public CleanEngine()
        {
            AppGlobals.EnableCleanEngineChanged += AppGlobals_EnableCleanEngineChanged;
            LoadJson();
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
            if (!newValue || !hasRan)
                LoadJson();
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
                    var action = item.GetProperty("action");
                    var type = action.GetProperty("type").GetString();
                    var path = action.GetProperty("path").GetString();
                    var patterns = action.TryGetProperty("patterns", out var patArray)
                        ? patArray.EnumerateArray().Select(p => p.GetString()!).ToList()
                        : [];

                    var requiredProcesses = new List<RequiredProcess>();
                    if (item.TryGetProperty("requiredProcesses", out var reqProcArray))
                    {
                        foreach (var proc in reqProcArray.EnumerateArray())
                        {
                            requiredProcesses.Add(
                                new RequiredProcess
                                {
                                    Name = proc.GetProperty("name").GetString()!,
                                    CanTerminate = proc.TryGetProperty("canTerminate", out var ct)
                                        ? ct.GetBoolean()
                                        : null,
                                }
                            );
                        }
                    }

                    items.Add(
                        new CleanupItem
                        {
                            Name = name!,
                            Description = item.TryGetProperty("description", out var desc)
                                ? desc.GetString()
                                : null,
                            Actions = new Action
                            {
                                Type = type!,
                                Path = path,
                                Patterns = patterns,
                            },
                            RequiredProcesses = requiredProcesses,
                            IsChecked = false,
                        }
                    );
                }
                Categories.Add(new CleanupCategory { Name = cat.Name, Items = items });
            }
        }

        [RelayCommand]
        private async Task CleanAsync()
        {
            TotalFreedText = 0;

            /* add only the selected items in the categories for cleaning */
            var selectedItems = Categories
                .SelectMany(c => c.Items)
                .Where(i => i.IsChecked)
                .ToList();

            if (selectedItems.Count == 0)
            {
                AddLog(
                    "At least one item has to be checked to start cleaning",
                    Colors.Orange
                );
                return;
            }

            /* check if any of the required processes are running */
            var runningProcs = Process
                .GetProcesses()
                .Select(p => p.ProcessName.ToLower())
                .ToHashSet();

            var toClose = new HashSet<string>();

            foreach (var item in selectedItems)
            {
                if (item.RequiredProcesses != null && item.RequiredProcesses.Count > 0)
                {
                    foreach (var proc in item.RequiredProcesses)
                    {
                        if (proc.CanTerminate == false || proc.CanTerminate is null)
                            if (
                                runningProcs.Contains(
                                    Path.GetFileNameWithoutExtension(proc.Name).ToLower()
                                )
                            )
                                toClose.Add(proc.Name);
                    }
                }
            }

            if (toClose.Count > 0)
            {
                AddLog(
                    "É necessário fechar as seguintes aplicações para continuar:\n"
                        + string.Join("\n", toClose),
                    (Color)Application.Current.Resources["SteelError"]
                );
                return;
            }

            StringBuilder log = new();
            long totalFreed = 0;

            await Task.Run(() =>
            {
                AppGlobals.EnableCleanEngine = false;
                AddLog(
                    "Starting Celer Cleaning Engine...",
                    (Color)Application.Current.Resources["SteelError"]
                );

                foreach (var item in selectedItems)
                {
                    long freed = 0;
                    if (item.Actions.Type == "folder-content")
                    {
                        string resolvedPath = Environment.ExpandEnvironmentVariables(
                            item.Actions.Path!
                        );
                        try
                        {
                            if (Directory.Exists(resolvedPath))
                            {
                                DeleteFolderContent(resolvedPath, ref freed, item.Name);
                                Interlocked.Add(ref totalFreed, freed);
                            }
                            else
                            {
                                AddLog(
                                    $"A pasta {resolvedPath} não existe ou não é válida.",
                                    Colors.Red
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            AddLog(
                                $"Falha ao apagar pasta: {resolvedPath}: {ex.Message}",
                                (Color)Application.Current.Resources["SteelError"]
                            );
                        }
                        continue;
                    }

                    if (item.Actions.Type == "content-pattern")
                    {
                        string resolvedPath = Environment.ExpandEnvironmentVariables(
                            item.Actions.Path!
                        );
                        try
                        {
                            if (Directory.Exists(resolvedPath))
                            {
                                foreach (var proc in item.RequiredProcesses ?? [])
                                {
                                    if (proc.CanTerminate == true && proc.Name == "explorer.exe")
                                    {
                                        Processes.KillExplorer();
                                    }
                                }
                                DeleteFilesWithPatterns(
                                    resolvedPath,
                                    item.Actions.Patterns!,
                                    ref freed,
                                    item.Name
                                );
                                Interlocked.Add(ref totalFreed, freed);
                            }
                            else
                            {
                                AddLog(
                                    $"A pasta {resolvedPath} não existe ou não é válida.",
                                    (Color)Application.Current.Resources["SteelError"]
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            AddLog(
                                $"Falha ao apagar ficheiros com padrões: {resolvedPath}: {ex.Message}",
                                (Color)Application.Current.Resources["SteelError"]
                            );
                        }
                        Processes.StartExplorer();
                        continue;
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
            hasRan = true;
            AppGlobals.EnableCleanEngine = true;
        }

        /// <summary>
        /// Deletes all files and folders in a specified directory recursively.
        /// </summary>
        /// <param name="resolvedPath">The path of the directory that we want to delete it's content</param>
        /// <param name="freed">To increment the total space saved</param>
        /// <param name="task">The name of the current task we are executing</param>
        public void DeleteFolderContent(string resolvedPath, ref long freed, string task)
        {
            var dir = new DirectoryInfo(resolvedPath);

            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    freed += file.Length;
                    file.Attributes = FileAttributes.Normal;
                    file.Delete();
                    AddLog($"Removido o ficheiro: {file.FullName}", Colors.YellowGreen);
                }
                catch (Exception ex)
                {
                    AddLog(
                        $"Falha ao apagar ficheiro: {file.FullName}: {ex.Message}",
                        (Color)Application.Current.Resources["SteelError"]
                    );
                }
            }

            foreach (
                var subDir in dir.GetDirectories("*", SearchOption.AllDirectories)
                    .OrderByDescending(d => d.FullName.Length)
            )
            {
                try
                {
                    subDir.Attributes = FileAttributes.Normal;
                    subDir.Delete(true);
                    AddLog($"Removida a pasta: {subDir.FullName}", Colors.Orange);
                }
                catch (Exception ex)
                {
                    AddLog(
                        $"Falha ao apagar pasta: {subDir.FullName}: {ex.Message}",
                        (Color)Application.Current.Resources["SteelError"]
                    );
                }
            }

            AddLog(
                $"A tarefa: {task} foi finalizada com sucesso",
                (Color)Application.Current.Resources["SteelSucess"]
            );
        }

        public void DeleteFilesWithPatterns(
            string resolvedPath,
            List<string> patterns,
            ref long freed,
            string task
        )
        {
            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(resolvedPath, pattern, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        freed += fileInfo.Length;
                        fileInfo.Attributes = FileAttributes.Normal;
                        fileInfo.Delete();
                        AddLog($"Removido o ficheiro: {file}", Colors.YellowGreen);
                    }
                    catch (Exception ex)
                    {
                        AddLog(
                            $"Falha ao apagar ficheiro: {file}: {ex.Message}",
                            (Color)Application.Current.Resources["SteelError"]
                        );
                    }
                }
            }
            AddLog(
                $"A tarefa: {task} foi finalizada com sucesso",
                (Color)Application.Current.Resources["SteelSucess"]
            );
        }

        public partial class CleanupItem : ObservableObject
        {
            public required string Name { get; set; }
            public string? Description { get; set; }
            public required Action Actions { get; set; }
            public List<RequiredProcess>? RequiredProcesses { get; set; }

            [ObservableProperty]
            private bool isChecked;
        }

        public class RequiredProcess
        {
            public required string Name { get; set; }
            public bool? CanTerminate { get; set; }
        }

        public class Action
        {
            public required string Type { get; set; }
            public string? Path { get; set; }
            public List<string>? Patterns { get; set; }
        }

        public class CleanupCategory
        {
            public required string Name { get; set; }
            public required ObservableCollection<CleanupItem> Items { get; set; }
        }
    }
}
