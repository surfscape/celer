using Celer.Models;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Celer.ViewModels
{
    public partial class CleanEngine : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<CleanupCategory> categories = [];
        [ObservableProperty] private string logText = string.Empty;
        [ObservableProperty] private double totalFreedText = 0;
        [ObservableProperty] private bool canClean = AppGlobals.EnableCleanEngine;
        [ObservableProperty] private object? selectedItem;

        public CleanEngine() { 
            LoadJson();
        }

        private void LoadJson()
        {
            const string path = "signatures.json";
            
            if (!File.Exists(path))
            {
                LogText = "Ficheiro de assinaturas de limpeza não encontrado, tenta atualizar as assinaturas através do menu > Ferramentas > Verificar Atualizações\n";
                AppGlobals.EnableCleanEngine = false;
            }
            try
            {
                LogText = "A carregar assinaturas...\n";
                var json = File.ReadAllText(path);
                ParseJson(json);
                AppGlobals.EnableCleanEngine = true;
                LogText += "Assinaturas carregadas com sucesso!\n";
            }
            catch(Exception e)
            {
                LogText = $"Ocorreu um erro a carregar as assinaturas: {e.Message}\n";
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
                        items.Add(new CleanupItem
                        {
                            Name = name,
                            Path = path,
                            RequiredProcesses = [.. item.GetProperty("requiredProcesses").EnumerateArray().Select(p => p.GetString()!)],
                            IsChecked = false
                        });
                    }
                }
                Categories.Add(new CleanupCategory { Name = cat.Name, Items = items });
            }
            CanClean = true;
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
                LogText += "É necessário selecionar pelo menos um item para iniciar a limpeza.\n";
                CanClean = true;
                return;
            }

            var runningProcs = Process.GetProcesses()
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
                LogText += "É necessário fechar as seguintes aplicações para continuar:\n" + string.Join("\n", toClose) + "\n";
                return;
            }

            StringBuilder log = new();
            long totalFreed = 0;

            await Task.Run(() =>
            {
                CanClean = false;
                LogText += "A iniciar Celer Cleaning Engine...\n";
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
                                    log.AppendLine($"Removido o ficheiro: {file.FullName}");
                                }
                                catch (Exception ex)
                                {
                                    log.AppendLine($"Falha ao apagar ficheiro: {file.FullName}: {ex.Message}");
                                }
                            }

                            try
                            {
                                dir.Delete(true);
                            }
                            catch (Exception ex)
                            {
                                log.AppendLine($"Falha ao apagar pasta: {resolvedPath}: {ex.Message}");
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

                        log.AppendLine($"Removido {resolvedPath} ({freed / 1024} KB)");
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"Falha ao apagar {resolvedPath}: {ex.Message}");
                    }

                    Interlocked.Add(ref totalFreed, freed);
                }
            });

            LogText += log.ToString();
            TotalFreedText = (long)(totalFreed / 1024f / 1024f);
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
