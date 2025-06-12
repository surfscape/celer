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
    }
}
