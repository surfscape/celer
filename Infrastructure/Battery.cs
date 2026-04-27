using Celer.Models.System.Battery;
using System.Diagnostics;
using System.Management;

namespace Celer.Infrastructure
{
    /// <summary>
    /// Provides static methods that retrieve battery information
    /// </summary>
    public class Battery
    {
        private const string CIMV2Class = "Win32_Battery";
        /// <summary>
        /// WMI Namespace where the additional battery WMI classes reside
        /// </summary>
        private const string BatteryClassesNamespace = "root\\WMI";
        private const string BatteryStaticDataClass = "BatteryStaticData";
        private const string BatteryCycleCountClass = "BatteryCycleCount";
        private const string BatteryRuntimeClass = "BatteryRuntime";

        /// <summary>
        /// Class that holds static information about the battery (Brand, Model, ID's, and FullDesignedCapacity)
        /// </summary>
        public BatteryInfo BatteryStaticData;
        /// <summary>
        /// Class that holds dynamic information about the battery (Capacity, Health, Charge Percentage, Power Status, etc)
        /// </summary>
        public BatteryStats? BatteryStats;
        public Battery()
        {
            using var win32Battery = new ManagementObjectSearcher($"SELECT Availability FROM {CIMV2Class}");
            if (win32Battery.Get().Cast<ManagementObject>().FirstOrDefault() == null)
                throw new NullReferenceException("The system does not have an available battery");
            BatteryStaticData = SetBatteryStaticData();
            Debug.WriteLine(BatteryStaticData.SerialNumber);
            Debug.WriteLine(BatteryStaticData.FullDesignCapacity);
            Debug.WriteLine(BatteryStaticData.Brand);
            Debug.WriteLine(BatteryStaticData.SerialNumber);
        }

        /// <summary>
        /// Populates the BatteryInfo class with the current Battery static data
        /// </summary>
        /// <returns>A new object of BatteryInfo with the system's static battery data</returns>
        /// <exception cref="NullReferenceException">Thrown if BatteryStaticData WMI is null</exception>
        private static BatteryInfo SetBatteryStaticData()
        {
            using var wimBatteryStaticData = new ManagementObjectSearcher(new ManagementScope(BatteryClassesNamespace), new ObjectQuery($"SELECT DesignedCapacity, DeviceName, ManufactureName, SerialNumber, UniqueID  FROM {BatteryStaticDataClass}"));
            var batterStaticData = wimBatteryStaticData.Get().Cast<ManagementObject>().FirstOrDefault();

            if (batterStaticData == null)
            {
                throw new NullReferenceException("The system does not provide static information about the battery");
            }
            else
                return new BatteryInfo((string)batterStaticData["ManufactureName"], (string)batterStaticData["DeviceName"], Convert.ToInt32(batterStaticData["SerialNumber"]), (string)batterStaticData["UniqueID"], Convert.ToInt32(batterStaticData["DesignedCapacity"]));
        }

        /* Methods below this comment are related to the BatteryStats class */

        /// <summary>
        /// Populates the BatteryStats class with current Battery data
        /// </summary>
        public void Update()
        {
            //BatteryStats = new BatteryStats("","","","","","",GetCycleCount(),BatteryStaticData.FullDesignCapacity);
        }

        /// <summary>
        /// Get current cycle count
        /// </summary>
        /// <returns>An int that represents the total cycle count the battery has gone through</returns>
        private static int GetCycleCount()
        {
            using var wimBatteryCycleCount = new ManagementObjectSearcher(new ManagementScope(BatteryClassesNamespace), new ObjectQuery($"SELECT CycleCount FROM {BatteryCycleCountClass}"));
            var batteryCycleCountData = wimBatteryCycleCount.Get().Cast<ManagementObject>().FirstOrDefault();
            if (batteryCycleCountData != null && batteryCycleCountData["CycleCount"] is int cycleCount)
                return cycleCount;
            return 0;
        }

        /// <summary>
        /// Get estimated runtime
        /// </summary>
        /// <returns>An instance of TimeSpan in seconds that represent the estimated runtime of the battery</returns>
        private static TimeSpan GetBatteryEstimatedRuntime()
        {
            using var wimBatteryEstimatedRuntime = new ManagementObjectSearcher(new ManagementScope(BatteryClassesNamespace), new ObjectQuery($"SELECT EstimatedRuntime FROM {BatteryRuntimeClass}"));
            var batteryEstimatedRunTime = wimBatteryEstimatedRuntime.Get().Cast<ManagementObject>().FirstOrDefault();
            if (batteryEstimatedRunTime != null && batteryEstimatedRunTime["EstimatedRuntime"] is int estimatedRuntime)
                return TimeSpan.FromSeconds(estimatedRuntime);
            return TimeSpan.FromSeconds(0);
        }

    }
}
