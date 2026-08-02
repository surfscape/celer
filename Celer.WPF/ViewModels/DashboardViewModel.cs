using Celer.Models.SystemInfo;
using Celer.Models;
using Celer.Properties;
using Celer.Services;
using Celer.Services.Memory;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Threading;
using Celer.Interfaces;

namespace Celer.ViewModels;

public partial class DashboardViewModel : BaseModuleViewModel, INavigationAware
{
    private bool _hasInitialized = false;
    private readonly NavigationService _navigationService;
    private readonly MemoryMonitorService _memoryService;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(MainConfiguration.Default.GeneralPollingRate) };

    private PerformanceCounter? _cpuCounter;
    private PerformanceCounter? _availableMemoryCounter;
    private List<PerformanceCounter>? _gpuCounters;

    private int _processUpdateCounter = 0;
    private const int PROCESS_UPDATE_INTERVAL = 5;
    private int _cachedProcessCount = 0;
    private int _cachedThreadCount = 0;
    private static bool _wmiInitialized = false;

    [ObservableProperty] private string? windowsVersion, cpuName, gpuName, gpuVendor;
    [ObservableProperty] private string gpuDriverVersion = "Unknown", gpuDirectXVersion = "Not Supported", gpuFeatureLevel = "Unavailable";
    [ObservableProperty] private double postTime, totalMemory, usedMemoryGraph, usedMemory, memoryUsage, cpuClockSpeed;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(UsedMemory))] private double availableMemory;
    [ObservableProperty] private float cpuUsage, gpuGeneralUsage;
    [ObservableProperty] private int processCount, threadCount;
    [ObservableProperty] public partial ObservableCollection<DiskInformation> DiskData { get; set; } = [];

    public DashboardViewModel(NavigationService navigationService)
    {
        _navigationService = navigationService;
        _memoryService = new MemoryMonitorService();
        _timer.Tick += async (s, e) => await UpdateSystemDataAsync();
    }

    private async Task UpdateSystemDataAsync()
    {
        try
        {
            await Task.Run(() =>
            {
                bool round = MainConfiguration.Default.EnableRounding;
                double Format(double val, int dec = 2) => round ? Math.Floor(val) : Math.Round(val, dec);

                if (_availableMemoryCounter != null)
                {
                    double available = _availableMemoryCounter.NextValue();
                    double used = TotalMemory - available;

                    AvailableMemory = Format(available, 0);
                    UsedMemory = Format(used, 0);
                    UsedMemoryGraph = Format(ValueHelpers.scaleToGraph(used, TotalMemory), 2);
                    MemoryUsage = Format((used / TotalMemory) * 100, 2);
                }

                if (_cpuCounter != null) CpuUsage = (float)Format(_cpuCounter.NextValue(), 1);

                GpuGeneralUsage = (float)Format(_gpuCounters?.Sum(c => c.NextValue()) ?? 0, 1);

                if (++_processUpdateCounter >= PROCESS_UPDATE_INTERVAL)
                {
                    _processUpdateCounter = 0;
                    UpdateProcessData();
                }
                else
                {
                    ProcessCount = _cachedProcessCount;
                    ThreadCount = _cachedThreadCount;
                }
            });
        }
        catch (Exception ex) { Debug.WriteLine($"Failed to update system data: {ex.Message}"); }
    }

    private void UpdateProcessData()
    {
        try
        {
            var processes = Process.GetProcesses();
            _cachedProcessCount = processes.Length;
            int totalThreads = 0;

            foreach (var process in processes)
            {
                try { totalThreads += process.Threads.Count; }
                catch { }
                finally { process.Dispose(); }
            }

            ProcessCount = _cachedProcessCount;
            ThreadCount = _cachedThreadCount = totalThreads;
        }
        catch (Exception ex) { Debug.WriteLine($"Error updating process data: {ex.Message}"); }
    }

    private void InitGpuCounters()
    {
        try
        {
            var category = new PerformanceCounterCategory("GPU Engine");
            _gpuCounters = category.GetInstanceNames()
                .Where(n => n.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase))
                .Select(name => new PerformanceCounter("GPU Engine", "Utilization Percentage", name))
                .ToList();

            _gpuCounters.ForEach(c => c.NextValue());
        }
        catch (Exception ex) { Debug.WriteLine($"Error init GPU counters: {ex.Message}"); }
    }

    private static string GetWindowsVersion()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            foreach (var item in searcher.Get()) return (string)item["Caption"];
        }
        catch (Exception ex) { Debug.WriteLine($"Error getting OS version: {ex.Message}"); }
        return "Microsoft Windows";
    }

    private static double GetPostTime()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\Session Manager\Power");
            if (key?.GetValue("FwPOSTTime") is int fwPostTimeMs)
            {
                double postTime = fwPostTimeMs / 1000.0;
                return MainConfiguration.Default.EnableRounding ? Math.Round(postTime, 1) : postTime;
            }
        }
        catch (Exception ex) { Debug.WriteLine($"Error getting POST time: {ex.Message}"); }
        return 0.0;
    }

    private void LoadCpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, MaxClockSpeed FROM Win32_Processor");
            foreach (var item in searcher.Get())
            {
                CpuName = item["Name"]?.ToString()?.Trim();
                CpuClockSpeed = Convert.ToDouble(item["MaxClockSpeed"]);
                break;
            }
        }
        catch (Exception ex) { Debug.WriteLine($"Error loading CPU info: {ex.Message}"); }
    }

    private void LoadGpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterCompatibility, DriverVersion, AdapterRAM FROM Win32_VideoController");
            var activeGpu = searcher.Get().Cast<ManagementObject>()
                .OrderByDescending(g => Convert.ToUInt64(g["AdapterRAM"] ?? 0))
                .FirstOrDefault();

            if (activeGpu != null)
            {
                GpuName = activeGpu["Name"].ToString();
                GpuVendor = activeGpu["AdapterCompatibility"].ToString();
                GpuDriverVersion = activeGpu["DriverVersion"].ToString()!;
            }
            LoadDxDiagInfo();
        }
        catch (Exception ex) { Debug.WriteLine($"Error loading GPU info: {ex.Message}"); }
    }

    private static string? _cachedDxDiagInfo;
    private static DateTime _dxDiagCacheTime = DateTime.MinValue;

    private void LoadDxDiagInfo()
    {
        try
        {
            if (!string.IsNullOrEmpty(_cachedDxDiagInfo) && (DateTime.UtcNow - _dxDiagCacheTime).TotalMilliseconds < 60000)
            {
                GpuDirectXVersion = _cachedDxDiagInfo;
                return;
            }

            if (File.Exists("dxdiag.xml"))
            {
                string xml = File.ReadAllText("dxdiag.xml");
                if (xml.Contains("DDIVersion"))
                {
                    GpuDirectXVersion = _cachedDxDiagInfo = XML.ExtractXmlValue(xml, "DDIVersion");
                    _dxDiagCacheTime = DateTime.UtcNow;
                    return;
                }
            }
        }
        catch {}

        GpuDirectXVersion = "N/A";
    }

    [RelayCommand]
    private void GetDriveInfo()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => MainConfiguration.Default.DISKS_ShowHiddenDrives || d.IsReady)
                .Select(d => new DiskInformation
                {
                    Format = d.DriveFormat,
                    Label = d.VolumeLabel ?? string.Empty,
                    Name = d.Name,
                    AvailableSpace = d.AvailableFreeSpace,
                    Size = d.TotalSize,
                    Type = d.DriveType.ToString(),
                    UsedSpace = d.TotalSize - d.TotalFreeSpace
                });

            DiskData = new ObservableCollection<DiskInformation>(drives);
        }
        catch (Exception ex) { Debug.WriteLine($"Error getting drive info: {ex.Message}"); }
    }

    [RelayCommand]
    private void NavigateToOptimization(string view) => _navigationService.Navigate(NavigationTabKey.Optimization, view);

    public async Task OnNavigatedTo() {
        if(!_hasInitialized) { 
        try
        {
            await Task.Run(() =>
            {
                var mem = _memoryService.GetMemoryInfo();
                TotalMemory = mem.TotalMemoryMB;

                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                _cpuCounter.NextValue();
                _availableMemoryCounter.NextValue();

                InitGpuCounters();

                if (!_wmiInitialized)
                {
                    WindowsVersion = GetWindowsVersion();
                    PostTime = GetPostTime();
                    LoadCpuInfo();
                    LoadGpuInfo();
                    _wmiInitialized = true;
                }
            });

            GetDriveInfo();
        }
        catch (Exception ex) { Debug.WriteLine($"Failed to initialize: {ex.Message}"); }
        finally { IsLoading = false; _timer.Start(); _hasInitialized = true; }
        } else
        {
            _timer.Start();
        }
    }
    public async Task OnNavigatedFrom()
    {
        _timer.Stop();
    }
}