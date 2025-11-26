namespace Celer.Services
{
    public class NavigationService
    {
        private readonly Dictionary<string, Action<string?>> _handlers = new();

        public void Register(string tabName, Action<string?> handler)
        {
            _handlers[tabName] = handler;
        }

        public Action<string, string?>? NavigateTo { get; set; }

        public string? CurrentTab { get; private set; }
        public string? CurrentInnerView { get; private set; }
        public event Action<string?, string?>? NavigationChanged;

        public bool CanGoBack =>
            !string.IsNullOrEmpty(CurrentInnerView) && !string.Equals(CurrentInnerView, "Main", StringComparison.Ordinal);

        public void Navigate(string tabName, string? innerViewName = null)
        {

            NavigateTo?.Invoke(tabName, innerViewName);
        }

        public void NavigateInternal(string tabName, string? innerViewName = null)
        {

            CurrentTab = tabName;
            CurrentInnerView = innerViewName;
            NavigationChanged?.Invoke(CurrentTab, CurrentInnerView);

            if (_handlers.TryGetValue(tabName, out var handler))
            {
                handler?.Invoke(innerViewName);
            }
        }

        public void BackToParent()
        {
            if (string.IsNullOrEmpty(CurrentTab))
                return;

            if (!CanGoBack)
                return;

            Navigate(CurrentTab, "Main");
        }
    }
}
