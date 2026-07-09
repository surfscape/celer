using System.Diagnostics;
using Microsoft.Management.Infrastructure;
using Celer.Infrastructure.Models.Battery;

namespace Celer.Infrastructure
{
    /// <summary>
    /// Celer Battery
    /// </summary>

    public class Battery : IDisposable
    {
        private const string CIMV2Namespace = "root\\cimv2";
        /// <summary>
        /// WMI Namespaces where the additional battery WMI classes reside
        /// </summary>
        private const string BatteryClassesNamespace = "root\\WMI";
        private const string BatteryStaticDataClass = "BatteryStaticData";
        private const string BatteryFullChargeClass = "BatteryFullChargedCapacity";
        private const string BatteryCycleCountClass = "BatteryCycleCount";
        private const string BatteryStatusClass = "BatteryStatus";
        private const string BatteryRuntimeClass = "BatteryRuntime";
        private const string BatteryWin32Class = "Win32_Battery";

        /// <summary>
        /// CIM Session used for all queries which is then disposed through IDisposable
        /// </summary>
        private readonly CimSession _session;

        /// <summary>
        /// Class that holds static information about the battery (Brand, Model, ID's, and FullDesignedCapacity)
        /// </summary>
        public BatteryInfo BatteryStaticData;
        /// <summary>
        /// Class that holds dynamic information about the battery (Capacity, Health, Charge Percentage, Power Status, etc), these can be updated with the Update() method
        /// </summary>
        public BatteryStats? BatteryStats;

        public Battery()
        {
            _session = CimSession.Create(null);

            using var win32Battery = _session.QueryInstances(CIMV2Namespace, "WQL", $"SELECT Availability FROM {BatteryWin32Class}").FirstOrDefault() ?? throw new NullReferenceException("The system does not have an available battery");
            BatteryStaticData = SetBatteryStaticData();
            Debug.WriteLine(GetBatteryPercentage());
        }

        /// <summary>
        /// Populates the BatteryInfo class with the current Battery static data
        /// </summary>
        /// <returns>A new object of BatteryInfo with the system's static battery data</returns>
        /// <exception cref="NullReferenceException">Thrown if BatteryStaticData WMI is null</exception>
        private BatteryInfo SetBatteryStaticData()
        {
            using var batteryStaticData = _session.QueryInstances(BatteryClassesNamespace, "WQL", $"SELECT DesignedCapacity, DeviceName, ManufactureName, SerialNumber, UniqueID FROM {BatteryStaticDataClass}").FirstOrDefault();

            if (batteryStaticData is null)
            {
                throw new NullReferenceException("The system does not provide static information about the battery");
            }
            else
            {
                return new BatteryInfo(
                    true,
                    (string)batteryStaticData.CimInstanceProperties["ManufactureName"].Value,
                    (string)batteryStaticData.CimInstanceProperties["DeviceName"].Value,
                    Convert.ToInt32(batteryStaticData.CimInstanceProperties["SerialNumber"].Value),
                    (string)batteryStaticData.CimInstanceProperties["UniqueID"].Value,
                    Convert.ToInt32(batteryStaticData.CimInstanceProperties["DesignedCapacity"].Value));
            }
        }

        /* Methods below this comment are related to the BatteryStats model */

        /// <summary>
        /// Populates the BatteryStats class with current Battery data
        /// </summary>
        public void Update()
        {
            BatteryStats = new BatteryStats(GetBatteryStatus() is 2 or 6, GetBatteryStatus() is 6, GetBatteryPercentage(), GetBatteryEstimatedRuntime(), GetRemainingFullCapacity(), GetRemainingChargeCapacity(), GetCycleCount(), BatteryStaticData.FullDesignCapacity);
        }

        /// <summary>
        /// Get current state of the battery (AC not charging, charging, discharging, etc)
        /// For what the return value means, check BatteryStatus on the Win32_Battery WMI documentation https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-battery
        /// </summary>
        /// <returns>An int that represents BatteryStatus</returns>
        private int GetBatteryStatus()
        {
            using var batteryChargingState = _session.QueryInstances(CIMV2Namespace, "WQL", $"SELECT BatteryStatus FROM {BatteryWin32Class}").FirstOrDefault();
            if (batteryChargingState == null)
                return 0;
            return Convert.ToInt32(batteryChargingState.CimInstanceProperties["BatteryStatus"].Value);
        }

        /// <summary>
        /// Get current charge percentage
        /// </summary>
        /// <returns>An int that represents the battery charge percentage</returns>
        private int GetBatteryPercentage()
        {
            using var batteryPercentage = _session.QueryInstances(CIMV2Namespace, "WQL", $"SELECT EstimatedChargeRemaining FROM {BatteryWin32Class}").FirstOrDefault();
            if (batteryPercentage == null)
                return 0;
            return Convert.ToInt32(batteryPercentage.CimInstanceProperties["EstimatedChargeRemaining"].Value);
        }

        /// <summary>
        /// Get remaining full charge capacity, unlike the DesignedFullCapacity, this is the capacity the battery is capable of holding when fully charged
        /// </summary>
        /// <returns>An int that represents the current full charge capacity in mWh</returns>
        private int GetRemainingFullCapacity()
        {
            using var batteryFullChargeCapacity = _session.QueryInstances(BatteryClassesNamespace, "WQL", $"SELECT FullChargedCapacity FROM {BatteryFullChargeClass}").FirstOrDefault();
            if (batteryFullChargeCapacity != null)
                return Convert.ToInt32(batteryFullChargeCapacity.CimInstanceProperties["FullChargedCapacity"].Value);
            return 0;
        }

        /// <summary>
        /// Get current charge capacity, this is the capacity the battery is currently holding
        /// </summary>
        /// <returns>An int that represents the current charge capacity in mWh</returns>
        private int GetRemainingChargeCapacity()
        {
            using var batteryChargeCapacity = _session.QueryInstances(BatteryClassesNamespace, "WQL", $"SELECT RemainingCapacity FROM {BatteryStatusClass}").FirstOrDefault();
            if (batteryChargeCapacity != null)
                return Convert.ToInt32(batteryChargeCapacity.CimInstanceProperties["RemainingCapacity"].Value);
            return 0;
        }

        /// <summary>
        /// Get current cycle count
        /// </summary>
        /// <returns>An int that represents the total cycle count the battery has gone through it's lifespan</returns>
        private int GetCycleCount()
        {
            using var batteryCycleCountData = _session.QueryInstances(BatteryClassesNamespace, "WQL", $"SELECT CycleCount FROM {BatteryCycleCountClass}").FirstOrDefault();
            if (batteryCycleCountData != null && batteryCycleCountData.CimInstanceProperties["CycleCount"].Value is int cycleCount)
                return cycleCount;
            return 0;
        }

        /// <summary>
        /// Get estimated runtime
        /// </summary>
        /// <returns>An instance of TimeSpan in seconds that represent the estimated runtime of the battery</returns>
        private TimeSpan GetBatteryEstimatedRuntime()
        {
            using var batteryEstimatedRunTime = _session.QueryInstances(BatteryClassesNamespace, "WQL", $"SELECT EstimatedRuntime FROM {BatteryRuntimeClass}").FirstOrDefault();
            uint rawMinutes = batteryEstimatedRunTime is not null ? Convert.ToUInt32(batteryEstimatedRunTime.CimInstanceProperties["EstimatedRuntime"].Value) : 0;
            if (rawMinutes == 0 || rawMinutes > 71582787 || (GetBatteryStatus() == 2 || GetBatteryStatus() == 6)) {
                return  TimeSpan.Zero;

            } else
                return TimeSpan.FromMinutes(rawMinutes);
        }

        public void Dispose()
        {
            _session.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
