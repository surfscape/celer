using ByteSizeLib;
using Celer.Models;
using Celer.Models.SystemInfo;
using Celer.Properties;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using static Celer.Views.Pages.Settings.SettingsModuleCleaningViewModel;
using Path = System.IO.Path;

namespace Celer.ViewModels
{
    public partial class CleanEngine : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CleanupCategory> categories = [];

        [ObservableProperty]
        public partial string TotalFreedText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial object? SelectedItem { get; set; }

        [ObservableProperty]
        public partial bool CanClean { get; set; } = false;

        public class LogBook()
        {
            public DateTime LogDate { get; set; } = DateTime.Now;
            public string LogEntry { get; set; } = string.Empty;
            public Brush LogColor { get; set; } = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush");
        }

        public ObservableCollection<LogBook> LogEntries { get; } = [];

        public CleanEngine()
        {
            WeakReferenceMessenger.Default.Register<TriggerCleaningSignaturesUpdate>(this, (r, m) =>
            {
                CanClean = m.Value;
                LoadJson();
            });
        }

        private void AddLog(string message, Brush foreground)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntries.Add(
                    new LogBook { LogDate = DateTime.Now, LogEntry = message, LogColor = foreground }
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
                CanClean = false;
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
                CanClean = false;
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
                    var actions = new List<Action>();
                    if (item.TryGetProperty("actions", out var actionArray))
                    {
                        foreach (var action in actionArray.EnumerateArray())
                            actions.Add(CreateAction(action));
                    } else if (item.TryGetProperty("action", out var action)) {
                            actions.Add(CreateAction(action));
                    }

                    items.Add(
                        new CleanupItem
                        {
                            Name = name!,
                            Description = item.TryGetProperty("description", out JsonElement desc)
                                ? desc.GetString() ?? string.Empty
                                : string.Empty,
                            Actions = actions,
                            RequiredProcesses = requiredProcesses,
                            IsChecked = false,
                        }
                    );
                }
                Categories.Add(new CleanupCategory { Name = cat.Name, Items = items });
            }
            var specialItems = new ObservableCollection<CleanupItem>();
            var customPathsFiles = new ObservableCollection<string>(MainConfiguration.Default.CLEANENGINE_CustomPaths?.Cast<string>() ?? []);
            if (customPathsFiles.Count > 0)
                specialItems.Add(new CleanupItem
                {
                    Name = "User Defined Files & Folders",
                    Description = "Delete files and folders added to the custom paths of the cleaning module settings page",
                    Actions = GetUserCustomFilesAndFolders(customPathsFiles),
                    IsChecked = false,
                });
            specialItems.Add(new CleanupItem
            {
                Name = "Disk Cleanup",
                Description= "Runs disk cleanup on the Windows drive and removes left over Windows Update packages",
                Actions = [new() { Type = ActionType.Command, Command = "cleanmgr.exe /d C: /VERYLOWDISK" }],
                IsChecked = false,
            });
            Categories.Add(
                    new CleanupCategory { Name = "Special", Items = specialItems }
            );
        }

        public static Action CreateAction(JsonElement action)
        {
            ActionType actionType = ActionType.Empty;
            if (action.GetProperty("type").ValueKind == JsonValueKind.String)
                actionType = GetActionTypeFromLegacyType(action.GetProperty("type").GetString()!);
            else
                actionType = (ActionType)action.GetProperty("type").GetInt16();

            return new Action
            {
                Type = actionType,
                Path = action.GetProperty("path").GetString(),
                Patterns = action.TryGetProperty("patterns", out var patArray)
                            ? patArray.EnumerateArray().Select(p => p.GetString()!).ToList()
                            : [],
            };
        }

        /// <summary>
        /// Receives the old action type in string and returns it's counterpart in the new enum format. This is to keep compatibility with cleaning signatures that use the old string to dictate action types, it's recommended to use the new int format
        /// </summary>
        /// <param name="stringType">The action type in the old string format</param>
        /// <returns>The enum counterpart of the string action type</returns>
        public static ActionType GetActionTypeFromLegacyType(string stringType)
        {
            return stringType switch
            {
                "folder-content" => ActionType.FolderContent,
                "file" => ActionType.File,
                "content-pattern" => ActionType.ContentPattern,
                "command" => ActionType.Command,
                _ => ActionType.Empty,
            };
        }
        private static List<Action> GetUserCustomFilesAndFolders(ObservableCollection<string> paths)
        {
            var actionList = new List<Action>();
            foreach (var item in paths)
            {
                FileAttributes attr = File.GetAttributes($@"{item}");

                if (attr.HasFlag(FileAttributes.Directory))
                    actionList.Add(new Action
                    {
                        Type = ActionType.FolderContent,
                        Path = item,
                    });
                else
                    actionList.Add(new Action
                    {
                        Type = ActionType.File,
                        Path = item,
                    });
            }
            return actionList;
        }

        [RelayCommand]
        private async Task CleanAsync()
        {
            TotalFreedText = string.Empty;

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
                SaveLog();
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
                SaveLog();
                return;
            }

            StringBuilder log = new();
            long totalFreed = 0;

            await Task.Run(async () =>
            {
                CanClean = false;
                AddLog(
                    "Starting Celer Cleaning Engine...",
                    (Brush)Application.Current.FindResource("SystemFillColorCautionBrush")
                );

                foreach (var item in selectedItems)
                {
                    long freed = 0;
                    foreach (var action in item.Actions)
                    {
                        if (action.Type == ActionType.FolderContent)
                        {
                            string resolvedPath = Environment.ExpandEnvironmentVariables(
                                action.Path!
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

                        if (action.Type == ActionType.ContentPattern)
                        {
                            string processHelper = string.Empty;
                            string resolvedPath = Environment.ExpandEnvironmentVariables(
                                action.Path!
                            );
                            try
                            {
                                if (Directory.Exists(resolvedPath))
                                {
                                    foreach (var proc in item.RequiredProcesses ?? [])
                                    {
                                        if (proc.CanTerminate && proc.Name == "explorer.exe")
                                        {
                                            processHelper = proc.Name;
                                            Processes.KillExplorer();
                                            await Task.Delay(300);
                                        }
                                    }
                                    DeleteFilesWithPatterns(
                                        resolvedPath,
                                        action.Patterns!,
                                        ref freed,
                                        item.Name
                                    );

                                    Interlocked.Add(ref totalFreed, freed);
                                    if (processHelper == "explorer.exe")
                                        Processes.StartExplorer();
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
                            continue;
                        }
                        if(action.Type == ActionType.Command && action.Command is not null)
                        {
                            long freeSpace = 0;
                            var cDrive = new DriveInfo("C");
                            AddLog($"Run command: {action.Command}", (Brush)Application.Current.FindResource("SystemFillColorSuccessBrush"));
                            try
                            {
                                var startInfo = new ProcessStartInfo("powershell.exe", $"{action.Command}")
                                {
                                    RedirectStandardOutput = true,
                                    CreateNoWindow = true,
                                    StandardOutputEncoding = Encoding.UTF8
                                };

                                using var process = new Process() { StartInfo = startInfo };
                                process.Start();
                                process.WaitForExit();

                                if (action.Command.Contains("cleanmgr", StringComparison.OrdinalIgnoreCase))
                                {
                                    Thread.Sleep(2000);
                                    while (Process.GetProcessesByName("cleanmgr").Length > 0)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                            catch (Exception e) {
                                AddLog($"Failed to run the command: {action.Command}\n{e.Message}", (Brush)Application.Current.FindResource("SystemFillColorCriticalBrush"));
                            }
                            finally
                            {
                                long finalFreeSpace = cDrive.TotalFreeSpace;
                                long spaceFreed = Math.Max(0, finalFreeSpace - freeSpace);
                            }
                            Interlocked.Add(ref totalFreed, freeSpace);
                            continue;
                        }
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
                            LogDate = DateTime.Now,
                            LogEntry = line,
                            LogColor = new SolidColorBrush(Colors.Green),
                        }
                    );
                }
            });
            TotalFreedText = ByteSize.FromBytes(totalFreed).ToString();
            CanClean = true;
            SaveLog();
        }

        public void SaveLog()
        {
            if (MainConfiguration.Default.CLEANENGINE_ExportLog)
            {
                string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string logFileName = "CelerCleaningLog.txt";
                LogEntries.Add(
                            new LogBook
                            {
                                LogDate = DateTime.Now,
                                LogEntry = $"Saved to log file: {logFilePath}\\{logFileName}",
                                LogColor = new SolidColorBrush(Colors.Green),
                            }
                        );
                using StreamWriter outputFile = new(Path.Combine(logFilePath, logFileName));
                foreach (LogBook logBook in LogEntries)
                    outputFile.WriteLine($"{logBook.LogDate}: {logBook.LogEntry}");
            }
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

        }

        public partial class CleanupItem : ObservableObject
        {
            public required string Name { get; set; }
            public string Description { get; set; } = string.Empty;
            public required List<Action> Actions { get; set; }
            public List<RequiredProcess>? RequiredProcesses { get; set; }

            [ObservableProperty]
            public partial bool IsChecked { get; set; } = false;
        }

        public class RequiredProcess
        {
            public required string Name { get; set; }
            public bool CanTerminate { get; set; }
        }

        public enum ActionType
        {
            FolderContent,
            File,
            ContentPattern,
            Command,
            Empty
        }

        public class Action
        {
            public required ActionType Type { get; set; }
            public string? Path { get; set; }
            public string? Command { get; set; }
            public List<string>? Patterns { get; set; }
        }

        public class CleanupCategory
        {
            public required string Name { get; set; }
            public required ObservableCollection<CleanupItem> Items { get; set; }
        }
    }
}
