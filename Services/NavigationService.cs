namespace Celer.Services
{
    public static class NavigationService
    {
        private static readonly Dictionary<string, Action<string>> _handlers = [];

        public static void Register(string tabName, Action<string> handler)
        {
            _handlers[tabName] = handler;
        }

        public static Action<string, string> NavigateTo;

        public static void Navigate(string tabName, string innerViewName = null)
        {
            NavigateTo?.Invoke(tabName, innerViewName);
        }

        public static void NavigateInternal(string tabName, string innerViewName = null)
        {
            if (_handlers.TryGetValue(tabName, out var handler))
            {
                handler?.Invoke(innerViewName);
            }
        }
    }
}
