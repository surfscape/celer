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
            items.Add(new StatusItem("Windows Defender", "Antivírus ativo", defenderActive));

            bool realTime = await DefenderHelper.IsRealTimeProtectionEnabledAsync();
            items.Add(new StatusItem("Proteção em tempo real", "Monitorização contínua", realTime));

            bool cloud = await DefenderHelper.IsCloudProtectionEnabledAsync();
            items.Add(new StatusItem("Proteção na cloud", "Reforço baseado na nuvem", cloud));

            bool ransomware = await DefenderHelper.IsRansomwareProtectionEnabledAsync();
            items.Add(
                new StatusItem(
                    "Proteção contra ransomware",
                    "Controla alterações a ficheiros",
                    ransomware
                )
            );

            bool sandboxed = await DefenderHelper.IsSandboxingEnabledAsync();
            items.Add(
                new StatusItem(
                    "Sandbox do Defender",
                    "Executa o antivírus de forma isolada",
                    sandboxed
                )
            );

            int score = CalculateScore(items, new[] { 3, 2, 1, 2, 2 });
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
