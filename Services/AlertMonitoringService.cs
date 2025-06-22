using Celer.Models;
using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Celer.Services
{
    public partial class AlertMonitoringService : ObservableObject, IDisposable
    {
        private readonly ObservableCollection<AlertModel> _alerts;
        private PerformanceCounter? _cpuCounter;
        private float _cpuThreshold;
        private float _memoryThreshold;
        private string _watchedProcessName;
        private float _processMemoryThresholdMB;

        [ObservableProperty]
        private bool isCpuTrackingEnabled;

        [ObservableProperty]
        private bool isMemoryTrackingEnabled;

        [ObservableProperty]
        private bool isProcessTrackingEnabled;

        public AlertMonitoringService(ObservableCollection<AlertModel> alerts)
        {
            _alerts = alerts;
            UpdateAllSettings();
            MainConfiguration.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        public void StartMonitoring()
        {
            Task.Run(async () =>
            {
                if (IsCpuTrackingEnabled && _cpuCounter != null)
                {
                    _cpuCounter.NextValue();
                }

                while (true)
                {
                    try
                    {
                        if (IsCpuTrackingEnabled)
                            CheckCPU();
                        if (IsMemoryTrackingEnabled)
                            CheckMemory();
                        if (IsProcessTrackingEnabled && !string.IsNullOrEmpty(_watchedProcessName))
                            CheckProcess();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in AlertMonitoringService loop: {ex.Message}");
                        await Task.Delay(5000);
                    }
                    await Task.Delay(2000);
                }
            });
        }

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case nameof(MainConfiguration.Default.ALERTS_CPUTrackingEnable):
                    IsCpuTrackingEnabled = MainConfiguration.Default.ALERTS_CPUTrackingEnable;
                    if (IsCpuTrackingEnabled)
                    {
                        _cpuCounter ??= new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    }
                    else
                    {
  
                        _cpuCounter?.Dispose();
                        _cpuCounter = null;
                        DispatchUpdateAlerts(() => {
                            var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.CPU);
                            if (existing != null) _alerts.Remove(existing);
                        });
                    }
                    break;

                case nameof(MainConfiguration.Default.ALERTS_CPUTrackingLimit):
                    _cpuThreshold = MainConfiguration.Default.ALERTS_CPUTrackingLimit;
                    break;

                case nameof(MainConfiguration.Default.ALERTS_MemoryTrackingEnable):
                    IsMemoryTrackingEnabled = MainConfiguration.Default.ALERTS_MemoryTrackingEnable;
                    if (!IsMemoryTrackingEnabled)
                    {
                        DispatchUpdateAlerts(() => {
                            var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.Memory);
                            if (existing != null) _alerts.Remove(existing);
                        });
                    }
                    break;

                case nameof(MainConfiguration.Default.ALERTS_EnableTrackProcess):
                    IsProcessTrackingEnabled = MainConfiguration.Default.ALERTS_EnableTrackProcess;
                    if (!IsProcessTrackingEnabled)
                    {
                        DispatchUpdateAlerts(() => {
                            var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.Process);
                            if (existing != null) _alerts.Remove(existing);
                        });
                    }
                    break;

                case nameof(MainConfiguration.Default.ALERTS_TrackProcess):
                    _watchedProcessName = MainConfiguration.Default.ALERTS_TrackProcess;
                    break;
            }
        }

        private void UpdateAllSettings()
        {
            IsCpuTrackingEnabled = MainConfiguration.Default.ALERTS_CPUTrackingEnable;
            IsMemoryTrackingEnabled = MainConfiguration.Default.ALERTS_MemoryTrackingEnable;
            IsProcessTrackingEnabled = MainConfiguration.Default.ALERTS_EnableTrackProcess;

            if (IsCpuTrackingEnabled)
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuThreshold = MainConfiguration.Default.ALERTS_CPUTrackingLimit;
            }

            if (IsMemoryTrackingEnabled)
            {
                _memoryThreshold = 80;
            }

            if (IsProcessTrackingEnabled)
            {
                _watchedProcessName = MainConfiguration.Default.ALERTS_TrackProcess;
                _processMemoryThresholdMB = 500;
            }
        }

        public void Dispose()
        {
            MainConfiguration.Default.PropertyChanged -= OnSettingsPropertyChanged;
            _cpuCounter?.Dispose();
            GC.SuppressFinalize(this);
        }


        private void DispatchUpdateAlerts(Action updateAction)
        {
            Application.Current?.Dispatcher.Invoke(updateAction);
        }

        private void CheckCPU()
        {
            if (_cpuCounter == null) return;
            float cpuUsage = _cpuCounter.NextValue();
            Thread.Sleep(500);
            cpuUsage = _cpuCounter.NextValue();
            DispatchUpdateAlerts(() =>
            {
                var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.CPU);
                if (cpuUsage >= _cpuThreshold)
                {
                    if (existing == null)
                    {
                        _alerts.Add(new AlertModel
                        {
                            Type = AlertType.CPU,
                            Title = $"CPU atingiu {cpuUsage:F1}%",
                            Description = "O uso de CPU está elevado. Feche aplicações pesadas para reduzir a carga.",
                            Icon = MahApps.Metro.IconPacks.PackIconLucideKind.Cpu,
                        });
                    }
                }
                else if (existing != null)
                {
                    _alerts.Remove(existing);
                }
            });
        }

        private void CheckMemory()
        {
            var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            var totalMemoryBytes = computerInfo.TotalPhysicalMemory;
            var availableMemoryBytes = computerInfo.AvailablePhysicalMemory;
            if (totalMemoryBytes == 0) return;
            float usedMemoryPercent = (float)((totalMemoryBytes - availableMemoryBytes) / (double)totalMemoryBytes * 100);
            DispatchUpdateAlerts(() =>
            {
                var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.Memory);
                if (usedMemoryPercent >= _memoryThreshold)
                {
                    if (existing == null)
                    {
                        _alerts.Add(new AlertModel
                        {
                            Type = AlertType.Memory,
                            Title = $"Memória atingiu {usedMemoryPercent:F1}%",
                            Description = "A memória RAM está quase cheia. Considere fechar aplicações que consomem muita memória.",
                            Icon = MahApps.Metro.IconPacks.PackIconLucideKind.MemoryStick,
                        });
                    }
                }
                else if (existing != null)
                {
                    _alerts.Remove(existing);
                }
            });
        }

        private void CheckProcess()
        {
            if (string.IsNullOrEmpty(_watchedProcessName)) return;
            Process[] processes = [];
            try
            {
                processes = Process.GetProcessesByName(_watchedProcessName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting process '{_watchedProcessName}': {ex.Message}");
                DispatchUpdateAlerts(() =>
                {
                    var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.Process);
                    if (existing != null) _alerts.Remove(existing);
                });
                return;
            }
            DispatchUpdateAlerts(() =>
            {
                var existing = _alerts.FirstOrDefault(a => a.Type == AlertType.Process);
                if (processes.Length > 0)
                {
                    var proc = processes[0];
                    try
                    {
                        using var counter = new PerformanceCounter("Process", "Working Set - Private", proc.ProcessName, true);
                        counter.NextValue();
                        Thread.Sleep(100);
                        float memoryMB = counter.NextValue() / (1024f * 1024f);
                        if (memoryMB >= _processMemoryThresholdMB)
                        {
                            if (existing == null)
                            {
                                _alerts.Add(new AlertModel
                                {
                                    Type = AlertType.Process,
                                    Title = $"'{_watchedProcessName}' está a utilizar {memoryMB:F1} MB",
                                    Description = $"O processo '{_watchedProcessName}' está a consumir muita memória.",
                                    Icon = MahApps.Metro.IconPacks.PackIconLucideKind.AppWindow,
                                });
                            }
                        }
                        else if (existing != null)
                        {
                            _alerts.Remove(existing);
                        }
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Instance") && ex.Message.Contains("does not exist"))
                    {
                        Debug.WriteLine($"Process instance for '{_watchedProcessName}' disappeared. {ex.Message}");
                        if (existing != null) _alerts.Remove(existing);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error reading process memory for '{_watchedProcessName}': {ex.Message}");
                        if (existing != null) _alerts.Remove(existing);
                    }
                    finally
                    {
                        foreach (var p in processes)
                            p.Dispose();
                    }
                }
                else if (existing != null)
                {
                    _alerts.Remove(existing);
                }
            });
        }
    }
}