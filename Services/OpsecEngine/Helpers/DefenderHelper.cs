namespace Celer.Services.OpsecEngine.Helpers
{
    using System.Diagnostics;
    using System.Text;

    public static class DefenderHelper
    {
        public static async Task<bool> IsDefenderEnabledAsync() =>
            await RunPowerShellCheckAsync(
                "Get-MpComputerStatus | Select-Object -ExpandProperty AMServiceEnabled"
            );

        public static async Task<bool> IsRealTimeProtectionEnabledAsync() =>
            await RunPowerShellCheckAsync(
                "Get-MpPreference | Select-Object -ExpandProperty DisableRealtimeMonitoring",
                invert: true
            );

        public static async Task<bool> IsCloudProtectionEnabledAsync() =>
            await RunPowerShellCheckAsync(
                "Get-MpPreference | Select-Object -ExpandProperty MAPSReporting",
                expectedValue: "2"
            );

        public static async Task<bool> IsRansomwareProtectionEnabledAsync() =>
            await RunPowerShellCheckAsync(
                "Get-MpPreference | Select-Object -ExpandProperty EnableControlledFolderAccess",
                expectedValue: "1"
            );

        public static async Task<bool> IsSandboxingEnabledAsync() =>
            await Task.FromResult(IsSandboxingEnabledViaRegistry());

        private static bool IsSandboxingEnabledViaRegistry()
        {
            const string path = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows Defender\Features";
            const string key = "AllowSandbox";
            return Convert.ToInt32(Microsoft.Win32.Registry.GetValue(path, key, 0) ?? 0) == 1;
        }

        private static async Task<bool> RunPowerShellCheckAsync(
            string command,
            bool invert = false,
            string? expectedValue = null
        )
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                },
            };

            try
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit();

                string result = output.Trim();

                if (expectedValue != null)
                    return result == expectedValue;

                if (bool.TryParse(result, out bool boolResult))
                    return invert ? !boolResult : boolResult;

                if (int.TryParse(result, out int intResult))
                    return invert ? intResult == 0 : intResult != 0;

                return false;
            }
            catch(ArgumentException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            catch(InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            catch(SystemException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
