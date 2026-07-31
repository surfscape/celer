using Celer.Infrastructure.Models.Battery;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Management.Infrastructure;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Celer.Infrastructure.Battery;

/// <summary>
/// Represents an individual battery device containing both static and dynamic information
/// </summary>
public partial class BatteryDevice(string instanceId, BatteryInfo staticData) : ObservableObject
{
    public string InstanceId { get; } = instanceId;   
    public BatteryInfo StaticData { get; } = staticData;
    [ObservableProperty]
    public partial BatteryStats? Stats { get; set; }
}

/// <summary>
/// Provides methods to retrive information about the system's batteries
/// </summary> 
public class Battery : IDisposable
{
    private const string CIMV2 = "root\\cimv2";
    private const string WMI = "root\\WMI";
    private readonly CimSession _session;

    public ObservableCollection<BatteryDevice> Batteries { get; } = [];

    public Battery()
    {
        _session = CimSession.Create(null);
        InitializeBatteries();
    }

    private void InitializeBatteries()
    {
        try
        {
            IEnumerable<CimInstance> staticInstances = _session.QueryInstances(WMI, "WQL", "SELECT InstanceName, DesignedCapacity, DeviceName, ManufactureName, SerialNumber, UniqueID FROM BatteryStaticData");

            foreach (var data in staticInstances)
            {
                string instanceName = (string)data.CimInstanceProperties["InstanceName"].Value;

                var info = new BatteryInfo(
                    true,
                    (string)data.CimInstanceProperties["ManufactureName"].Value,
                    (string)data.CimInstanceProperties["DeviceName"].Value,
                    Convert.ToInt32(data.CimInstanceProperties["SerialNumber"].Value),
                    (string)data.CimInstanceProperties["UniqueID"].Value,
                    Convert.ToInt32(data.CimInstanceProperties["DesignedCapacity"].Value)
                );

                Batteries.Add(new BatteryDevice(instanceName, info));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing batteries: {ex.Message}");
        }

        if (Batteries.Count == 0)
        {
            Debug.WriteLine("No batteries detected on this system.");
        }
    }

    public void Update()
    {
        if (Batteries.Count == 0) return;
        var win32 = _session.QueryInstances(CIMV2, "WQL", "SELECT BatteryStatus, EstimatedChargeRemaining FROM Win32_Battery").ToList();
        var fullCapacities = _session.QueryInstances(WMI, "WQL", "SELECT InstanceName, FullChargedCapacity FROM BatteryFullChargedCapacity").ToList();
        var statuses = _session.QueryInstances(WMI, "WQL", "SELECT InstanceName, RemainingCapacity FROM BatteryStatus").ToList();
        var cycleCounts = _session.QueryInstances(WMI, "WQL", "SELECT InstanceName, CycleCount FROM BatteryCycleCount").ToList();
        var runtimes = _session.QueryInstances(WMI, "WQL", "SELECT InstanceName, EstimatedRuntime FROM BatteryRuntime").ToList();

        for (int i = 0; i < Batteries.Count; i++)
        {
            var battery = Batteries[i];
            string id = battery.InstanceId;

            var w32 = win32.ElementAtOrDefault(i);
            int status = w32 != null ? Convert.ToInt32(w32.CimInstanceProperties["BatteryStatus"].Value) : 0;
            int percentage = w32 != null ? Convert.ToInt32(w32.CimInstanceProperties["EstimatedChargeRemaining"].Value) : 0;

            bool isCharging = status is 2 or 6;
            bool isCritical = status is 6;

            int fullCap = GetMatchedValue<int>(fullCapacities, id, "FullChargedCapacity");
            int currCap = GetMatchedValue<int>(statuses, id, "RemainingCapacity");
            int cycles = GetMatchedValue<int>(cycleCounts, id, "CycleCount");
            uint rawTime = GetMatchedValue<uint>(runtimes, id, "EstimatedRuntime");

            TimeSpan runtime = (rawTime == 0 || rawTime > 71582787 || isCharging)
                ? TimeSpan.Zero
                : TimeSpan.FromMinutes(rawTime / 2.0);

            battery.Stats = new BatteryStats(
                isCharging, isCritical, percentage, runtime, fullCap,
                currCap, cycles, battery.StaticData.FullDesignCapacity
            );
        }

        win32.ForEach(x => x.Dispose());
        fullCapacities.ForEach(x => x.Dispose());
        statuses.ForEach(x => x.Dispose());
        cycleCounts.ForEach(x => x.Dispose());
        runtimes.ForEach(x => x.Dispose());
    }

    /// <summary>
    /// Helper to find the correct CIM instance by its hardware InstanceName
    /// </summary>
    private static T GetMatchedValue<T>(IEnumerable<CimInstance> instances, string instanceName, string property)
    {
        var match = instances.FirstOrDefault(x => (string)x.CimInstanceProperties["InstanceName"].Value == instanceName);
        if (match?.CimInstanceProperties[property]?.Value is IConvertible val)
        {
            return (T)Convert.ChangeType(val, typeof(T));
        }
        return default!;
    }

    public void Dispose()
    {
        _session.Dispose();
        GC.SuppressFinalize(this);
    }
}