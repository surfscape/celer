using Celer.Models.Protector;
using Celer.Services.OpsecEngine.Helpers;

namespace Celer.Services.OpsecEngine
{
    public static class SecurityEvaluator
    {
        public static async Task<(List<StatusItem> Items, int Score)> EvaluateAsync()
        {
            var items = new List<StatusItem>();

            bool defenderActive = await DefenderHelper.IsDefenderEnabledAsync();
            items.Add(new StatusItem("Windows Defender", "Protection enabled", defenderActive));

            bool realTime = await DefenderHelper.IsRealTimeProtectionEnabledAsync();
            items.Add(new StatusItem("Real time protection", "Allow Windows Defender to scan the system in real time", realTime));

            bool cloud = await DefenderHelper.IsCloudProtectionEnabledAsync();
            items.Add(new StatusItem("Cloud-based Protection", "Use a lightweight cloud-based scanner", cloud));

            bool ransomware = await DefenderHelper.IsRansomwareProtectionEnabledAsync();
            items.Add(
                new StatusItem(
                    "Protection against ransomware",
                    "Block suspicious programs that can lock and ransom files",
                    ransomware
                )
            );

            bool sandboxed = await DefenderHelper.IsSandboxingEnabledAsync();
            items.Add(
                new StatusItem(
                    "Defender Sandbox",
                    "Allow Windows Defender to run in a seperate environment for self protection",
                    sandboxed
                )
            );

            int score = CalculateScore(items, new[] { 3, 3, 1, 2, 2 });
            return (items, score);
        }

        private static int CalculateScore(List<StatusItem> items, int[] weights)
        {
            int maxScore = weights.Sum();
            int actualScore = items.Select((item, i) => item.IsEnabled ? weights[i] : 0).Sum();
            return (int)Math.Round((double)actualScore / maxScore * 10);
        }
    }
}
