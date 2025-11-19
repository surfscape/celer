namespace Celer.Models
{
    public static class AppGlobals
    {
        private static bool _enableCleanEngine = false;

        public static event EventHandler? EnableCleanEngineChanged;

        public static bool EnableCleanEngine
        {
            get => _enableCleanEngine;
            set
            {
                if (_enableCleanEngine != value)
                {
                    _enableCleanEngine = value;
                    EnableCleanEngineChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        private static bool _workInProgress = false;
        public static event EventHandler? WorkInProgressChanged;

        public static bool WorkInProgress
        {
            get => _workInProgress;
            set
            {
                if (_workInProgress != value)
                {
                    _workInProgress = value;
                    WorkInProgressChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }
    }
}
