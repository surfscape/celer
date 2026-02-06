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
        private bool canClean = AppGlobals.EnableCleanEngine;

        private bool hasRan;

        public class LogBook()
        {
            public string LogEntry { get; set; } = string.Empty;
            public Brush LogColor { get; set; } = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush");
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
            if (newValue != oldValue)
                LoadJson();
        }

        private void AddLog(string message, Brush foreground)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(
                    new LogBook { LogEntry = message, LogColor = foreground }
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
                    "Signatures not found. Update them through the Tools menu and click Check Updates",
                    (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
                );
                AppGlobals.EnableCleanEngine = false;
                return;
            }
            try
            {
                AddLog(
                    "Loading signatures...",
                    (Brush)Application.Current.FindResource("SystemFillColorAttentionBrush")
                );
                var json = File.ReadAllText(path);
                ParseJson(json);
                AddLog("Signatures loaded sucessfully!", (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush"));
            }
            catch (Exception e)
            {
                AddLog($"An error occurred when loading the signaturs: {e.Message}", (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush"));
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
                                    CanTerminate = proc.TryGetProperty("canTerminate", out var ct) && ct.GetBoolean(),
                                }
                            );
                        }
                    }

                    items.Add(
                        new CleanupItem
                        {
                            Name = name!,
                            Description = item.TryGetProperty("description", out JsonElement desc)
                                ? desc.GetString() ?? string.Empty
                                : string.Empty,
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
                    (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
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
                if (item.RequiredProcesses != null && item.RequiredProcesses.Count > 0 && runningProcs is not null)
                {
                    foreach (var proc in item.RequiredProcesses)
                    {
                        if (!proc.CanTerminate && runningProcs.Contains(Path.GetFileNameWithoutExtension(proc.Name).ToLower()))
                           toClose.Add(proc.Name);
                    }
                }
            }

            if (toClose.Count > 0)
            {
                AddLog(
                    "The following application have to be closed to proceed with the cleaning process:\n"
                        + string.Join("\n", toClose),
                    (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
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
                    (Brush)Application.Current.FindResource("SystemFillColorCautionBrush")
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
                                    $"The folder {resolvedPath} does not exist or is invalid",
                                    (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            AddLog(
                                $"Exception while trying to delete the folder {resolvedPath}: {ex.Message}",
                                (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
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
                                   $"The folder {resolvedPath} does not exist or is invalid",
                                   (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
                               );
                            }
                        }
                        catch (Exception ex)
                        {
                            AddLog(
                                $"Exception while trying to delete the folder {resolvedPath} with content pattern: {ex.Message}",
                                (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
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
        /// <param name="resolvedPath">The path of the directory that we want to delete the contents of</param>
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
                    AddLog($"Deleted the file: {file.FullName}", (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush"));
                }
                catch (Exception ex)
                {
                    AddLog(
                        $"Exception when deleting file {file.FullName}: {ex.Message}",
                        (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
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
                    AddLog($"Deleted folder {subDir.FullName}", (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush"));
                }
                catch (Exception ex)
                {
                    AddLog(
                        $"Exception when deleting folder {subDir.FullName}: {ex.Message}",
                        (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
                    );
                }
            }
            AddLog(
                $"The task: {task} was terminated successfully",
                (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush")
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
                        AddLog($"Deleted the file {file}", (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush"));
                    }
                    catch (Exception ex)
                    {
                        AddLog(
                            $"Exception when deleting file {file}: {ex.Message}",
                            (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush")
                        );
                    }
                }
            }
            AddLog(
                $"The task: {task} was terminated successfully",
                (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush")
            );
        }

        public partial class CleanupItem : ObservableObject
        {
            public required string Name { get; set; }
            public string Description { get; set; } = string.Empty;
            public required Action Actions { get; set; }
            public List<RequiredProcess>? RequiredProcesses { get; set; }

            [ObservableProperty]
            private bool isChecked;
        }

        public class RequiredProcess
        {
            public required string Name { get; set; }
            public bool CanTerminate { get; set; }
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
