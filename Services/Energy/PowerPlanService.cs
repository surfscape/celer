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
                if (line.Contains(':'))
                {
                    var parts = line.Trim().Split(':');
                    var guid = parts[0].Trim().Split(' ')[0];
                    var name = parts[1].Trim().Trim('*').Trim();
                    plans.Add(new PowerPlan { GUID = guid, Name = name });
                }
            }

            return plans;
        }

        public PowerPlan? GetActivePowerPlan()
        {
            var output = ExecuteCmd("powercfg /getactivescheme");
            var guid = output.Split(':')[0].Trim().Split(' ')[0];
            var name = output.Split(':')[1].Trim();
            return new PowerPlan { GUID = guid, Name = name };
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
            };

            using var process = Process.Start(startInfo);
            return process?.StandardOutput.ReadToEnd() ?? string.Empty;
        }
    }
}
