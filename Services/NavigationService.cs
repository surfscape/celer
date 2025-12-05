namespace Celer.Services
{
    public class NavigationService
    {
        private readonly Dictionary<string, Action<string?>> _handlers = [];
        private readonly Dictionary<string, string?> _tabInnerViews = [];

        public void Register(string tabName, Action<string?> handler)
        {
            _handlers[tabName] = handler;
            _tabInnerViews[tabName] = null;
        }

        public Action<string, string?>? NavigateTo { get; set; }

        public string? CurrentTab { get; private set; }

        public string? CurrentInnerView
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentTab))
                    return null;

                return _tabInnerViews.TryGetValue(CurrentTab, out var v) ? v : null;
            }
        }

        public event Action<string?, string?>? NavigationChanged;

        public bool CanGoBack
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentTab))
                    return false;

                if (!_tabInnerViews.TryGetValue(CurrentTab, out var inner))
                    return false;

                return !string.IsNullOrEmpty(inner) && !string.Equals(inner, "Main", StringComparison.Ordinal);
            }
        }

        public void Navigate(string tabName, string? innerViewName = null)
        {
            NavigateTo?.Invoke(tabName, innerViewName);
        }

        public void NavigateInternal(string tabName, string? innerViewName = null)
        {
            _tabInnerViews[tabName] = innerViewName;
            CurrentTab = tabName;

            var currentInner = _tabInnerViews.TryGetValue(tabName, out var v) ? v : null;
            NavigationChanged?.Invoke(CurrentTab, currentInner);

            if (_handlers.TryGetValue(tabName, out var handler))
            {
                handler?.Invoke(currentInner);
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

        public string? GetInnerViewForTab(string tabName)
        {
            return _tabInnerViews.TryGetValue(tabName, out var v) ? v : null;
        }
    }
}
