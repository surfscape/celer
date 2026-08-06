namespace Celer.Services.OpsecEngine.Helpers
{
    public static class RegistryHelper
    {
        public static Task<bool> IsTelemetryDisabledAsync() =>
            Task.FromResult(
                GetRegDWORD(
                    @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                    "AllowTelemetry"
                ) == 0
            );

        public static Task<bool> AreBackgroundAppsDisabledAsync() =>
            Task.FromResult(
                GetRegDWORD(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
                    "GlobalUserDisabled"
                ) == 1
            );

        public static Task<bool> IsAdvertisingIdDisabledAsync() =>
            Task.FromResult(
                GetRegDWORD(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
                    "DisabledByGroupPolicy"
                ) == 1
            );

        public static Task<bool> IsLocationDisabledAsync() =>
            Task.FromResult(
                GetRegDWORD(
                    @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lfsvc\Service\Configuration",
                    "Status"
                ) == 0
            );

        private static int GetRegDWORD(string path, string name) =>
            Convert.ToInt32(Microsoft.Win32.Registry.GetValue(path, name, 1));
    }
}
