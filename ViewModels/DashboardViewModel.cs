﻿using Celer.Models;
using Celer.Models.SystemInfo;
using Celer.Properties;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Threading;

namespace Celer.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly NavigationService _navigationService;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _availableMemoryCounter;

    /// <summary>
    /// Used to track if the dashboard is loading data and to show a loading bar if true
    /// </summary>
    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private string? windowsVersion;

    [ObservableProperty]
    private double postTime;

    [ObservableProperty]
    private float cpuUsage;

    [ObservableProperty]
    private double totalMemory;

    [ObservableProperty]
    private double usedMemory;

    [ObservableProperty]
    private double memoryUsage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UsedMemory))]
    private double availableMemory;

    [ObservableProperty]
    private ObservableCollection<DiskInformation> diskData = [];

    [ObservableProperty]
    private ObservableCollection<AlertModel> alerts = [];

    [ObservableProperty]
    private bool enableAlerts = false;

    /// <summary>
    /// CPU Data
    /// </summary>
    [ObservableProperty]
    private string? cpuName;

    [ObservableProperty]
    private double cpuClockSpeed;

    [ObservableProperty]
    private int processCount;

    [ObservableProperty]
    private int threadCount;

    [ObservableProperty]
    private string cpuStatus;

    /// <summary>
    /// GPU Data
    /// </summary>
    [ObservableProperty]
    private string? gpuName;

    [ObservableProperty]
    private string? gpuVendor;

    [ObservableProperty]
    private string gpuDriverVersion = "Unknown";

    [ObservableProperty]
    private string gpuDirectXVersion = "Not Supported";

    [ObservableProperty]
    private string gpuFeatureLevel = "Unavailable";

    [ObservableProperty]
    private float gpuGeneralUsage;

    public DashboardViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;
        MainConfiguration.Default.PropertyChanged += OnSettingsChanged;
        _timer.Tick += async (s, e) => await UpdateSystemDataAsync();
    }

    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
        var propertiesToWatch = new[]
        {
        nameof(MainConfiguration.Default.ALERTS_CPUTrackingEnable),
        nameof(MainConfiguration.Default.ALERTS_MemoryTrackingEnable),
        nameof(MainConfiguration.Default.ALERTS_EnableTrackProcess)
    };

        if (propertiesToWatch.Contains(e.PropertyName))
        {
            EnableAlertTracking();
        }
    }

    private void EnableAlertTracking()
    {
        if (
    MainConfiguration.Default.ALERTS_CPUTrackingEnable
    || MainConfiguration.Default.ALERTS_MemoryTrackingEnable
    || MainConfiguration.Default.ALERTS_EnableTrackProcess
)
        {
            EnableAlerts = true;
            var alertService = new AlertMonitoringService(Alerts);
            alertService.StartMonitoring();
        }
        else
        {
            EnableAlerts = false;
        }
    }


    public async Task InitializeAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                _cpuCounter.NextValue();

                WindowsVersion = GetWindowsVersion();
                PostTime = GetPostTime();
                TotalMemory = GetTotalMemory();

                LoadCpuInfo();
                LoadGpuInfo();
            });

            GetDriveInfo();
            EnableAlertTracking();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro na inicialização: {ex}");
        }
        finally
        {
            IsLoading = false;
            _timer.Start();
        }
    }

    private async Task UpdateSystemDataAsync()
    {
        try
        {
            await Task.Run(async () =>
            {
                if (_availableMemoryCounter != null)
                {
                    AvailableMemory = _availableMemoryCounter.NextValue();
                    UsedMemory = TotalMemory - AvailableMemory;
                    MemoryUsage = Math.Round((UsedMemory / TotalMemory) * 100, 2);
                }

                if (_cpuCounter != null)
                {
                    CpuUsage = (float)Math.Round(_cpuCounter.NextValue(), 1);
                }

                ProcessCount = Process.GetProcesses().Length;
                ThreadCount = Process
                    .GetProcesses()
                    .Sum(p =>
                    {
                        try
                        {
                            return p.Threads.Count;
                        }
                        catch
                        {
                            return 0;
                        }
                    });

                GpuGeneralUsage = await GetGpuUsageAsync();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating system data: {ex.Message}");
        }
    }

    private void LoadGpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, AdapterCompatibility, DriverVersion, AdapterRAM FROM Win32_VideoController"
            );
            var gpus = searcher.Get().Cast<ManagementObject>();

            var activeGpu = gpus.OrderByDescending(g => Convert.ToUInt64(g["AdapterRAM"] ?? 0))
                .FirstOrDefault();
            if (activeGpu != null)
            {
                GpuName = activeGpu["Name"].ToString();
                GpuVendor = activeGpu["AdapterCompatibility"].ToString();
                GpuDriverVersion = activeGpu["DriverVersion"].ToString()!;
            }

            LoadDxDiagInfo();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error loading GPU info: " + ex.Message);
        }
    }

    private static double GetTotalMemory()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"
            );
            using var collection = searcher.Get();
            foreach (var item in collection)
            {
                var result = (ulong)item["TotalVisibleMemorySize"];
                return result / 1024.0;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting total memory: {ex.Message}");
        }
        return 0;
    }

    private static string GetWindowsVersion()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Caption FROM Win32_OperatingSystem"
            );
            using var collection = searcher.Get();
            foreach (var item in collection)
            {
                return (string)item["Caption"];
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting Windows version: {ex.Message}");
        }
        return "Microsoft Windows";
    }

    private static double GetPostTime()
    {
        try
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(
                @"System\CurrentControlSet\Control\Session Manager\Power"
            )!;
            if (key != null)
            {
                var o = key.GetValue("FwPOSTTime");
                if (o != null)
                {
                    int fwPostTimeMs = Convert.ToInt32(o);
                    return fwPostTimeMs / 1000.0;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting POST time: {ex.Message}");
        }
        return 0.0;
    }

    private void LoadCpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, MaxClockSpeed, Status FROM Win32_Processor"
            );
            foreach (var item in searcher.Get())
            {
                CpuName = item["Name"]?.ToString()?.Trim();
                CpuClockSpeed = Convert.ToDouble(item["MaxClockSpeed"]);
                CpuStatus = item["Status"]?.ToString() ?? "N/A";
                break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error loading CPU info: " + ex.Message);
        }
    }

    private void GetDriveInfo()
    {
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo driveInfo in allDrives)
        {
            if (MainConfiguration.Default.DISKS_ShowHiddenDrives)
            {
                DiskData.Add(
                    new DiskInformation
                    {
                        Format = driveInfo.DriveFormat,
                        Label = driveInfo.VolumeLabel,
                        Name = driveInfo.Name,
                        AvailableSpace = driveInfo.AvailableFreeSpace,
                        Size = driveInfo.TotalSize,
                        Type = driveInfo.DriveType.ToString(),
                        UsedSpace = driveInfo.TotalSize - driveInfo.TotalFreeSpace,
                    }
                );
            }
            else
            {
                if (driveInfo.IsReady)
                {
                    DiskData.Add(
                        new DiskInformation
                        {
                            Format = driveInfo.DriveFormat,
                            Label = driveInfo.VolumeLabel,
                            Name = driveInfo.Name,
                            AvailableSpace = driveInfo.AvailableFreeSpace,
                            Size = driveInfo.TotalSize,
                            Type = driveInfo.DriveType.ToString(),
                            UsedSpace = driveInfo.TotalSize - driveInfo.TotalFreeSpace,
                        }
                    );
                }
            }
        }
    }

    private void LoadDxDiagInfo()
    {
        try
        {
            string xml = File.ReadAllText("dxdiag.xml");

            if (xml.Contains("DDIVersion"))
            {
                GpuDirectXVersion = XML.ExtractXmlValue(xml, "DDIVersion");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error loading DxDiag info: " + ex.Message);
            GpuDirectXVersion = "N/A";
        }
    }

    public static async Task<float> GetGpuUsageAsync()
    {
        var category = new PerformanceCounterCategory("GPU Engine");

        var instanceNames = category
            .GetInstanceNames()
            .Where(n => n.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var counters = instanceNames
            .Select(name => new PerformanceCounter("GPU Engine", "Utilization Percentage", name))
            .ToArray();
        foreach (var counter in counters)
        {
            _ = counter.NextValue();
        }

        await Task.Delay(1000);

        float usage = counters.Sum(c => c.NextValue());

        foreach (var counter in counters)
        {
            counter.Dispose();
        }

        return usage;
    }

    [RelayCommand]
    private void RefreshDisks()
    {
        // Clear DiskData object since the default way to insert data is to add instead of updating
        DiskData = [];
        GetDriveInfo();
    }

    [RelayCommand]
    private void NavigateToOptimization(string view)
    {
        _navigationService.Navigate("Otimizacao", view);
    }
}
