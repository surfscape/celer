using Celer.Interfaces;
using Celer.Models;
using Celer.Properties;

namespace Celer.Services
{
    /// <summary>
    /// Singleton service that manages navigation between tabs and corresponding subviews. It tracks the current history stack, registered tab viewmodels and views, and provides lifecycle callbacks to viewmodels implementing <see cref="INavigationAware"/>.
    /// </summary>
    public class NavigationService
    {
        /// <summary>
        /// Callbacks used to switch the active subview of a tab. Only for tabs that host subviews
        /// (see <see cref="ViewModels.BaseNavigationViewModel"/>) register here.
        /// </summary>
        private readonly Dictionary<NavigationTabKey, Func<string?, Task>> _subviewHosts = [];

        /// <summary>
        /// The viewmodel backing each tab. Any of these implementing <see cref="INavigationAware"/>
        /// receive tab level lifecycle callbacks independently of whether they host subviews.
        /// </summary>
        private readonly Dictionary<NavigationTabKey, object> _tabViewModels = [];

        private readonly Dictionary<NavigationTabKey, Stack<string?>> _tabStacks = [];

        private NavigationTabKey? _activeTab;
        private string? _activeInnerView;
        private bool _hasActivated;

        /// <summary>
        /// Registers a tab's viewmodel so it can take part in the navigation lifecycle.
        /// </summary>
        public void RegisterTab(NavigationTabKey tabKey, object viewModel)
        {
            _tabViewModels[tabKey] = viewModel;

            if (!_tabStacks.ContainsKey(tabKey))
            {
                var stack = new Stack<string?>();
                stack.Push(null);
                _tabStacks[tabKey] = stack;
            }
        }

        /// <summary>
        /// Registers the callback used to switch subviews within a tab. Only tabs that actually
        /// have subviews need this; tab level lifecycle is handled by <see cref="RegisterTab"/>.
        /// </summary>
        public void RegisterSubviewHost(NavigationTabKey tabKey, Func<string?, Task> handler)
        {
            _subviewHosts[tabKey] = handler;
            if (!_tabStacks.ContainsKey(tabKey))
                _tabStacks[tabKey] = new Stack<string?>();
            _tabStacks[tabKey].Clear();
            _tabStacks[tabKey].Push(null);
        }

        public Func<NavigationTabKey, string?, Task>? NavigateTo { get; set; }

        public NavigationTabKey? CurrentTab { get; private set; }

        public string? CurrentInnerView
        {
            get
            {
                if (CurrentTab == null)
                    return null;

                var stack = _tabStacks.TryGetValue(CurrentTab.Value, out var s) ? s : null;
                return stack != null && stack.Count > 0 ? stack.Peek() : null;
            }
        }

        public event Action<NavigationTabKey?, string?>? NavigationChanged;

        private bool _compactMode = MainConfiguration.Default.SaveSidebarCompactMode && MainConfiguration.Default.SidebarCompactMode;
        public bool CompactMode
        {
            get => _compactMode;
            set
            {
                if (_compactMode != value)
                {
                    _compactMode = value;
                    CompactModeChanged?.Invoke(this, _compactMode);
                    if (MainConfiguration.Default.SaveSidebarCompactMode)
                    {
                        MainConfiguration.Default.SidebarCompactMode = value;
                        MainConfiguration.Default.Save();
                    }
                }
            }
        }

        public event EventHandler<bool>? CompactModeChanged;

        public bool CanGoBack
        {
            get
            {
                if (CurrentTab == null)
                    return false;

                if (!_tabStacks.TryGetValue(CurrentTab.Value, out var stack))
                    return false;

                var inner = stack.Count > 0 ? stack.Peek() : null;
                return !string.IsNullOrEmpty(inner) && !string.Equals(inner, "Main", StringComparison.Ordinal);
            }
        }

        public Task Navigate(NavigationTabKey tabKey, string? innerViewName = null)
        {
            if (NavigateTo != null)
                return NavigateTo(tabKey, innerViewName);

            return NavigateInternal(tabKey, innerViewName);
        }

        public async Task NavigateInternal(NavigationTabKey tabKey, string? innerViewName = null)
        {
            string? targetInner =
                string.IsNullOrEmpty(innerViewName) || innerViewName == "Main" ? null : innerViewName;

            if (_hasActivated && _activeTab == tabKey && _activeInnerView == targetInner)
                return;

            if (!_tabStacks.TryGetValue(tabKey, out var stack))
                _tabStacks[tabKey] = stack = new Stack<string?>();

            if (targetInner is null)
            {
                stack.Clear();
                stack.Push(null);
            }
            else if (stack.Count == 0 || stack.Peek() != targetInner)
            {
                stack.Push(targetInner);
            }

            if (_activeTab is { } previousTab && previousTab != tabKey)
                await NotifyNavigatedFrom(previousTab);

            bool tabChanged = _activeTab != tabKey;

            CurrentTab = tabKey;
            _activeTab = tabKey;
            _activeInnerView = stack.Count > 0 ? stack.Peek() : null;
            _hasActivated = true;

            NavigationChanged?.Invoke(CurrentTab, _activeInnerView);

            if (_subviewHosts.TryGetValue(tabKey, out var host))
                await host(_activeInnerView);

            if (tabChanged)
                await NotifyNavigatedTo(tabKey);
        }

        private async Task NotifyNavigatedTo(NavigationTabKey tabKey)
        {
            if (_tabViewModels.TryGetValue(tabKey, out var vm) && vm is INavigationAware aware)
                await aware.OnNavigatedTo();
        }

        private async Task NotifyNavigatedFrom(NavigationTabKey tabKey)
        {
            if (_tabViewModels.TryGetValue(tabKey, out var vm) && vm is INavigationAware aware)
                await aware.OnNavigatedFrom();
        }

        public Task BackToParent()
        {
            if (CurrentTab == null)
                return Task.CompletedTask;

            if (!CanGoBack)
                return Task.CompletedTask;

            var stack = _tabStacks[CurrentTab.Value];
            if (stack.Count > 1)
                stack.Pop();

            var tab = CurrentTab.Value;
            var currentInner = stack.Peek();

            _activeInnerView = currentInner;

            NavigationChanged?.Invoke(tab, currentInner);

            if (_subviewHosts.TryGetValue(tab, out var host))
            {
                return host(currentInner);
            }

            return Task.CompletedTask;
        }

        public string? GetInnerViewForTab(NavigationTabKey tabKey)
        {
            if (!_tabStacks.TryGetValue(tabKey, out var stack) || stack.Count == 0)
                return null;

            return stack.Peek();
        }
    }
}
