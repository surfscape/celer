using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
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
                if(!info.IsCharging)
                {
                    info.EstimatedTime = runtime > 0 ? TimeSpan.FromMinutes(runtime) : TimeSpan.Zero;
                }

                var report = GenerateBatteryReport();
                info.Health = GetBatteryHealthPercentageFromReport(report);
            }
            catch
            {
                info.HasBattery = false;
            }

            return info;
        }


        public string GenerateBatteryReport()
        {
            var reportPath = "batteryreport.xml";

            if (!File.Exists(reportPath))
            {

                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/BATTERYREPORT /OUTPUT \"batteryreport.xml\" /XML",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(psi)?.WaitForExit();
            }


            return reportPath;
        }

        public static string ExtractXmlValue(string xml, string elementName)
        {
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://schemas.microsoft.com/battery/2012";

            var element = doc.Descendants(ns + elementName).FirstOrDefault();
            return element?.Value;
        }


        public static int GetBatteryHealthPercentageFromReport(string reportPath)
        {
            if(!File.Exists(reportPath))
            {
                Trace.WriteLine(":(");
                return -1;
            }

            var xml = File.ReadAllText(reportPath);


            var designStr = ExtractXmlValue(xml, "DesignCapacity");
            var fullStr = ExtractXmlValue(xml, "FullChargeCapacity");


            if (double.TryParse(designStr, out var design) &&
                double.TryParse(fullStr, out var full) &&
                design > 0)
            {
                
                return (int)((full / design) * 100);   
            }

            return -1;
        }


    }

}
