using System.Diagnostics;
using System.IO;
using System.Management;
using System.Xml.Linq;

namespace Celer.Services.Energy
{
    public class BatteryInfo
    {
        public bool HasBattery { get; set; }
        public int Percentage { get; set; }
        public bool IsCharging { get; set; }
        public TimeSpan EstimatedTime { get; set; }
        public int Health { get; set; }

        public int RemainingCapacity { get; set; }

        public int ChargedCapacity { get; set; }

        public int FactoryCapacity { get; set; }


        public int FactoryCapacityPercentage
        {
            get
            {
                if (FactoryCapacity == 0) return 0;
                return (int)((double)RemainingCapacity / FactoryCapacity * 100);
            }
        }

        public int ChargedCapacityPercentage
        {
            get
            {
                if (FactoryCapacity == 0) return 0;
                return (int)((double)ChargedCapacity / FactoryCapacity * 100);
            }
        }
    }

    public class BatteryService
    {
        public BatteryInfo GetBatteryInfo()
        {
            var info = new BatteryInfo();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                var battery = searcher.Get().Cast<ManagementObject>().FirstOrDefault();

                if (battery == null)
                {
                    info.HasBattery = false;
                    return info;
                }

                info.HasBattery = true;
                info.Percentage = Convert.ToInt32(battery["EstimatedChargeRemaining"]);
                info.IsCharging = Convert.ToUInt16(battery["BatteryStatus"]) is 2 or 6;

                var runtime = Convert.ToUInt32(battery["EstimatedRunTime"]);
                if (!info.IsCharging)
                {
                    info.EstimatedTime =
                        runtime > 0 ? TimeSpan.FromMinutes(runtime) : TimeSpan.Zero;
                }
                var (health, capacity, factoryCapacity, chargedCapacity) = GetBatteyHealthInfo("batteryreport.xml", info.Percentage);
                info.Health = health;
                info.RemainingCapacity = capacity;
                info.FactoryCapacity = factoryCapacity;
                info.ChargedCapacity = chargedCapacity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                info.HasBattery = false;
            }

            return info;
        }

        public static string ExtractXmlValue(string xml, string elementName)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://schemas.microsoft.com/battery/2012";

            var element = doc.Descendants(ns + elementName).FirstOrDefault();
            return element?.Value;
        }

        public static (int health, int currentCapacity, int factoryCapacity, int chargeCapacity) GetBatteyHealthInfo(
            string reportPath, int percentage
        )
        {
            if (!File.Exists(reportPath))
            {
                return (0, 0, 0, 0);
            }

            var xml = File.ReadAllText(reportPath);

            var designStr = ExtractXmlValue(xml, "DesignCapacity");
            var fullStr = ExtractXmlValue(xml, "FullChargeCapacity");
            if (
                double.TryParse(designStr, out var design)
                && double.TryParse(fullStr, out var full)
                && design > 0
            )
            {
                return ((int)((full / design) * 100), int.Parse(fullStr), int.Parse(designStr), ((int.Parse(fullStr) * percentage) / 100));
            }

            return (0, 0, 0, 0);
        }
    }
}
