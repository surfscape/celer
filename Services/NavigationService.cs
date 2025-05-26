namespace Celer.Services
{
    public class NavigationService
    {
        private readonly Dictionary<string, Action<string>> _handlers = new();

        public void Register(string tabName, Action<string> handler)
        {
            _handlers[tabName] = handler;
        }

        public Action<string, string>? NavigateTo { get; set; }

        public void Navigate(string tabName, string? innerViewName = null)
        {
            NavigateTo?.Invoke(tabName, innerViewName);
        }

        public void NavigateInternal(string tabName, string? innerViewName = null)
        {
            if (_handlers.TryGetValue(tabName, out var handler))
            {
                handler?.Invoke(innerViewName);
            }
        }
    }
}
