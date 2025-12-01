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
                    "Telemetry",
                    "Send diagnostic data to Microsoft",
                    isTelemetryDisabled
                )
            );

            bool areAppsBlocked = await RegistryHelper.AreBackgroundAppsDisabledAsync();
            items.Add(
                new StatusItem(
                    "Background applicaions",
                    "Disable the executation of apps in the background",
                    areAppsBlocked
                )
            );

            bool adsDisabled = await RegistryHelper.IsAdvertisingIdDisabledAsync();
            items.Add(
                new StatusItem("Targeted Ads", "Show targeted ads by using an ID", adsDisabled)
            );

            bool locationDisabled = await RegistryHelper.IsLocationDisabledAsync();
            items.Add(
                new StatusItem(
                    "Location Services",
                    "Block location tracking",
                    locationDisabled
                )
            );

            int score = CalculateScore(items, [3, 1, 2, 2]);
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
