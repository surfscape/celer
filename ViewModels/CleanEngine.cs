using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Celer.ViewModels
{
    public partial class CleanEngine : ObservableObject
    {
        // private const string JsonUrl = "https://surfscape.github.io/services/celer_v1/signatures/clean.json";
        private const string LocalJson = "cleanup.json";

        [ObservableProperty] private ObservableCollection<CleanupCategory> categories = [];
        [ObservableProperty] private string logText = string.Empty;
        [ObservableProperty] private double totalFreedText = 0;
        [ObservableProperty] private bool canClean = false;
        [ObservableProperty] private object? selectedItem;


        public CleanEngine()
        {
            LoadJson();
        }

        private void LoadJson()
        {
            try
            {
                /* using var client = new HttpClient();
                 var json = await client.GetStringAsync(JsonUrl);
                 File.WriteAllText(LocalJson, json);
                 ParseJson(json);*/
                var json = File.ReadAllText(LocalJson);
                ParseJson(json);
            }
            catch
            {
                if (File.Exists(LocalJson))
                {
                    var json = File.ReadAllText(LocalJson);
                    ParseJson(json);
                }
                else
                {
                    LogText = "No JSON data available. Cleaning disabled.";
                    CanClean = false;
                }
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
                    items.Add(new CleanupItem
                    {
                        Name = item.GetProperty("name").GetString(),
                        Path = item.GetProperty("path").GetString(),
                        RequiredProcesses = [.. item.GetProperty("requiredProcesses").EnumerateArray().Select(p => p.GetString())],
                        IsChecked = false
                    });
                }
                Categories.Add(new CleanupCategory { Name = cat.Name, Items = items });
            }
            CanClean = true;
        }
        [RelayCommand]
        private async Task CleanAsync()
        {
            LogText += ("A iniciar Celer Clean Engine...");
            TotalFreedText = 0;

            long totalFreed = 0;
            var runningProcs = Process.GetProcesses().Select(p => p.ProcessName.ToLower()).ToHashSet();
            var toClose = new HashSet<string>();

            foreach (var category in Categories)
            {
                foreach (var item in category.Items.Where(i => i.IsChecked))
                {
                    foreach (var proc in item.RequiredProcesses)
                    {
                        if (runningProcs.Contains(Path.GetFileNameWithoutExtension(proc).ToLower()))
                            toClose.Add(proc);
                    }
                }
            }

            if (toClose.Count > 0)
            {
                LogText = "Please close the following applications:\n" + string.Join("\n", toClose);
                return;
            }

            StringBuilder log = new();

            await Task.Run(() =>
            {
                CanClean = false;
                foreach (var category in Categories)
                {
                    foreach (var item in category.Items.Where(i => i.IsChecked))
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
                                        log.AppendLine($"Removido o ficheiro: {file.FullName}\n");
                                        LogText += log.ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        log.AppendLine($"Falha ao apagar ficheiro: {file.FullName}: {ex.Message}\n");
                                        LogText += log.ToString();
                                    }
                                }

                                try
                                {
                                    dir.Delete(true);
                                }
                                catch (Exception ex)
                                {
                                    log.AppendLine($"Falha ao apagar pasta: {resolvedPath}: {ex.Message}");
                                    LogText += log.ToString();
                                }
                            }
                            else if (File.Exists(resolvedPath))
                            {
                                var fileInfo = new FileInfo(resolvedPath)
                                {
                                    Attributes = FileAttributes.Normal
                                };
                                freed += fileInfo.Length;
                                fileInfo.Delete();
                            }

                            totalFreed += freed;
                            log.AppendLine($"Removido {resolvedPath} ({freed / 1024} KB)");
                            LogText += log.ToString();
                        }
                        catch (Exception ex)
                        {
                            log.AppendLine($"Falha ao apagar {resolvedPath}: {ex.Message}");
                            LogText += log.ToString();
                        }
                    }
                }
            });
            TotalFreedText = (long)(totalFreed / 1024f / 1024f);
        }


        public partial class CleanupItem : ObservableObject
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public List<string> RequiredProcesses { get; set; }

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
