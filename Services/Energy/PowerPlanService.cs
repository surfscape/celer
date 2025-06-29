using System.Diagnostics;

namespace Celer.Services.Energy
{
    public class PowerPlan
    {
        public string GUID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class PowerPlanService
    {
        public List<PowerPlan> GetAllPowerPlans()
        {
            var plans = new List<PowerPlan>();
            var output = ExecuteCmd("powercfg /list");
            var lines = output.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Power Scheme GUID") || trimmed.StartsWith("Esquema de Energia GUID"))
                {
                    var parts = trimmed.Split(':');
                    if (parts.Length < 2) continue;

                    var afterGuid = parts[1].Trim();
                    var guidPart = afterGuid.Split(' ')[0];
                    var namePartStart = afterGuid.IndexOf('(');
                    var namePartEnd = afterGuid.IndexOf(')');

                    string name = (namePartStart >= 0 && namePartEnd > namePartStart)
                        ? afterGuid.Substring(namePartStart + 1, namePartEnd - namePartStart - 1)
                        : "Desconhecido";

                    plans.Add(new PowerPlan
                    {
                        GUID = guidPart,
                        Name = name
                    });
                }
            }

            return plans;
        }

        public PowerPlan? GetActivePowerPlan()
        {
            var output = ExecuteCmd("powercfg /getactivescheme");
            var line = output.Trim();
            if (line.Contains(':'))
            {
                var parts = line.Split(':');
                if (parts.Length < 2) return null;

                var afterGuid = parts[1].Trim();
                var guidPart = afterGuid.Split(' ')[0];
                var namePartStart = afterGuid.IndexOf('(');
                var namePartEnd = afterGuid.IndexOf(')');

                string name = (namePartStart >= 0 && namePartEnd > namePartStart)
                    ? afterGuid.Substring(namePartStart + 1, namePartEnd - namePartStart - 1)
                    : "N/A";

                return new PowerPlan
                {
                    GUID = guidPart,
                    Name = name
                };
            }

            return null;
        }

        public void SetActivePowerPlan(string guid)
        {
            ExecuteCmd($"powercfg /setactive {guid}");
        }

        private string ExecuteCmd(string cmd)
        {
            var startInfo = new ProcessStartInfo("cmd", $"/c {cmd}")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            process.WaitForExit();
            return process?.StandardOutput.ReadToEnd() ?? string.Empty;
        }
    }
}
