using Windows.Win32;
using Windows.Win32.System.Threading;

namespace Celer.Utilities
{
    public static class ProcessPowerManager
    {
        private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

        public static bool IsSupported =>
            Environment.OSVersion.Version >= new Version(10, 0, 22000);

        public static bool Enable()
        {
            if (!IsSupported) return false;

            bool throttled = SetPowerThrottling(
                controlMask: PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                stateMask: PROCESS_POWER_THROTTLING_EXECUTION_SPEED
            );

            PInvoke.SetPriorityClass(
                PInvoke.GetCurrentProcess(),
                PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS);

            return throttled;
        }

        public static bool Disable()
        {
            if (!IsSupported) return false;

            bool throttled = SetPowerThrottling(
                controlMask: PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
                stateMask: 0
            );

            PInvoke.SetPriorityClass(
                PInvoke.GetCurrentProcess(),
                PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS);

            return throttled;
        }

        public static bool Reset()
        {
            if (!IsSupported) return false;

            bool throttled = SetPowerThrottling(controlMask: 0, stateMask: 0);

            PInvoke.SetPriorityClass(
                PInvoke.GetCurrentProcess(),
                PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS);

            return throttled;
        }

        private static bool SetPowerThrottling(uint controlMask, uint stateMask)
        {
            var state = new PROCESS_POWER_THROTTLING_STATE
            {
                Version = 1,
                ControlMask = controlMask,
                StateMask = stateMask
            };

            unsafe
            {
                return PInvoke.SetProcessInformation(
                    PInvoke.GetCurrentProcess(),
                    PROCESS_INFORMATION_CLASS.ProcessPowerThrottling,
                    &state,
                    (uint)sizeof(PROCESS_POWER_THROTTLING_STATE));
            }
        }
    }
}
