using Celer.Models.Protector;
using Celer.Services.OpsecEngine.Helpers;

namespace Celer.Services.OpsecEngine
{
    public static class PrivacyEvaluator
    {
        public static async Task<(List<StatusItem> Items, int Score)> EvaluateAsync()
        {
            var items = new List<StatusItem>();

            bool isTelemetryDisabled = await RegistryHelper.IsTelemetryDisabledAsync();
            items.Add(
                new StatusItem(
                    "Telemetria",
                    "Envio de dados para a Microsoft",
                    !isTelemetryDisabled
                )
            );

            bool areAppsBlocked = await RegistryHelper.AreBackgroundAppsDisabledAsync();
            items.Add(
                new StatusItem(
                    "Apps em segundo plano",
                    "Evita execução não autorizada",
                    areAppsBlocked
                )
            );

            bool adsDisabled = await RegistryHelper.IsAdvertisingIdDisabledAsync();
            items.Add(
                new StatusItem("Publicidade personalizada", "Controla rastreio por ID", adsDisabled)
            );

            bool locationDisabled = await RegistryHelper.IsLocationDisabledAsync();
            items.Add(
                new StatusItem(
                    "Serviços de localização",
                    "Impede rastreio de localização",
                    locationDisabled
                )
            );

            int score = CalculateScore(items, new[] { 3, 2, 1, 2 });
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
