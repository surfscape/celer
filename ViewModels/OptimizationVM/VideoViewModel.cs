using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Threading;

namespace Celer.ViewModels.OptimizationVM
{
    public partial class VideoViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private ObservableCollection<GpuInfo> gpus = [];

        [ObservableProperty]
        private GpuInfo? selectedGpu;

        private readonly Computer gpu = new() { IsGpuEnabled = true };

        private readonly DispatcherTimer _gpuUpdateTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1),
        };

        public VideoViewModel()
        {
            _gpuUpdateTimer.Tick += (s, e) => UpdateGpuSensors();
        }

        public async Task Initialize()
        {
            try
            {
                gpu.Open();
                await LoadGpusAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    $"Error initializing Celer video engine: {e.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Debug.WriteLine(e);
            }
            finally
            {
                IsLoading = false;
                _gpuUpdateTimer.Start();
            }
        }

        private async Task LoadGpusAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Gpus.Clear();
            });

            var wmiGpus = await Task.Run(() => GetGpusFromWmi());
            var dxDiagGpus = await Task.Run(() => ParseDxDiag());

            foreach (var gpu in wmiGpus)
            {
                var dx = dxDiagGpus.FirstOrDefault(d =>
                    gpu.Name.Contains(d.Name, StringComparison.OrdinalIgnoreCase)
                );

                if (dx != null)
                {
                    gpu.DirectXVersion = dx.DirectXVersion;
                    gpu.WddmVersion = dx.WddmVersion;
                    gpu.IsWhqlLogoPresent = dx.IsWhqlLogoPresent;
                    gpu.SupportsHDR = dx.SupportsHDR;
                    gpu.SupportsParavirtualization = dx.SupportsParavirtualization;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Gpus.Add(gpu);
                });
            }
            SelectedGpu = Gpus.FirstOrDefault();
        }

        private static List<GpuInfo> GetGpusFromWmi()
        {
            var result = new List<GpuInfo>();

            using var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_VideoController"
            );
            foreach (ManagementObject mo in searcher.Get())
            {
                var name = mo["Name"]?.ToString() ?? "Desconhecido";
                var manufacturer = mo["AdapterCompatibility"]?.ToString() ?? "Desconhecido";
                var totalMemory = (UInt32)(mo["AdapterRAM"] ?? 0);

                UInt32? dedicated = null;
                UInt32? shared = null;

                try
                {
                    dedicated = Convert.ToUInt32(mo["AdapterRAM"] ?? 0) / (1024 * 1024);
                    shared = Convert.ToUInt32(mo["SharedSystemMemory"] ?? 0) / (1024 * 1024);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error when parsing adapter memory {e}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine(e);
                }

                var gpuInfo = new GpuInfo
                {
                    Name = name,
                    Manufacturer = manufacturer,
                    DriverVersion = mo["DriverVersion"]?.ToString() ?? "0.0.0",
                    MemoryTotalMb = totalMemory / (1024 * 1024),
                    DedicatedMemoryMb = dedicated,
                    SharedMemoryMb = shared,
                    MemoryUsedMb = 0,
                    IsInternal =
                        name.Contains("intel", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("radeon graphics", StringComparison.OrdinalIgnoreCase),
                };

                result.Add(gpuInfo);
            }

            return result;
        }

        private static List<GpuInfo> ParseDxDiag()
        {
            var list = new List<GpuInfo>();
            var path = "dxdiag.xml";

            try
            {
                if (!File.Exists(path))
                    return list;

                var doc = File.ReadAllText(path);

                list.Add(
                    new GpuInfo
                    {
                        Name = XML.ExtractXmlValue(doc, "CardName"),
                        DirectXVersion = XML.ExtractXmlValue(doc, "DDIVersion"),
                        WddmVersion = XML.ExtractXmlValue(doc, "DriverModel"),
                        IsWhqlLogoPresent =
                            XML.ExtractXmlValue(doc, "DriverWHQLLogo")?.Contains("Yes") ?? false,
                        SupportsHDR =
                            XML.ExtractXmlValue(doc, "HDRSupport")?.Contains("Supported") ?? false,
                        SupportsParavirtualization =
                            XML.ExtractXmlValue(doc, "VirtualGPUSupport")
                                ?.Contains("Paravirtualization") ?? false,
                    }
                );
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error parsing dxDiag {e}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine(e);
            }

            return list;
        }

        private void UpdateGpuSensors()
        {
            gpu.Accept(new GpuMonitor());

            foreach (
                var hw in gpu.Hardware.Where(h =>
                    h.HardwareType == HardwareType.GpuNvidia
                    || h.HardwareType == HardwareType.GpuAmd
                    || h.HardwareType == HardwareType.GpuIntel
                )
            )
            {
                hw.Update();

                var matchedGpu = Gpus.FirstOrDefault(g =>
                    hw.Name.Contains(g.Name, StringComparison.OrdinalIgnoreCase)
                    || g.Name.Contains(hw.Name, StringComparison.OrdinalIgnoreCase)
                );

                if (matchedGpu is null)
                    continue;

                foreach (var sensor in hw.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                    {
                        matchedGpu.GpuUsage = sensor.Value ?? 0;
                    }

                    if (
                        sensor.SensorType == SensorType.SmallData
                        && sensor.Name.Contains("GPU Memory Used")
                    )
                    {
                        matchedGpu.MemoryUsedMb = (ulong)(sensor.Value ?? 0);
                    }
                }
            }

            gpu.Close();
        }

        public async Task StartTimerAsync()
        {
            if (!_gpuUpdateTimer.IsEnabled)
            {
                await LoadGpusAsync();
                _gpuUpdateTimer.Start();
            }
        }

        public async Task StopTimerAsync()
        {
            _gpuUpdateTimer.Stop();
            await Task.Run(() => gpu.Close());
        }

        public partial class GpuInfo : ObservableObject
        {
            [ObservableProperty]
            private float gpuUsage;

            [ObservableProperty]
            private ulong memoryUsedMb;
            public string Name { get; set; } = "Desconhecido";
            public string Manufacturer { get; set; } = "Desconhecido";
            public string DriverVersion { get; set; } = "0.0.0";
            public UInt32 MemoryTotalMb { get; set; }
            public UInt32? DedicatedMemoryMb { get; set; }
            public UInt32? SharedMemoryMb { get; set; }
            public bool IsInternal { get; set; }
            public string? DirectXVersion { get; set; }
            public string? WddmVersion { get; set; }
            public bool? IsWhqlLogoPresent { get; set; }
            public bool? SupportsHDR { get; set; }
            public bool? SupportsParavirtualization { get; set; }
        }
    }
}
