using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Celer.Properties;
using Celer.Models;

namespace Celer.Services
{
    public class NavigationService
    {
        private readonly Dictionary<NavigationTabKey, Func<string?, Task>> _handlers = new();
        private readonly Dictionary<NavigationTabKey, Stack<string?>> _tabStacks = new();

        public void Register(NavigationTabKey tabKey, Func<string?, Task> handler)
        {
            _handlers[tabKey] = handler;
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
            if (!_tabStacks.ContainsKey(tabKey))
                _tabStacks[tabKey] = new Stack<string?>();

            if (string.IsNullOrEmpty(innerViewName) || innerViewName == "Main")
            {
                _tabStacks[tabKey].Clear();
                _tabStacks[tabKey].Push(null);
            }
            else
            {
                var stack = _tabStacks[tabKey];
                if (stack.Count == 0 || stack.Peek() != innerViewName)
                {
                    stack.Push(innerViewName);
                }
            }

            CurrentTab = tabKey;

            var currentInner = _tabStacks[tabKey].Count > 0 ? _tabStacks[tabKey].Peek() : null;
            NavigationChanged?.Invoke(CurrentTab, currentInner);

            if (_handlers.TryGetValue(tabKey, out var handler))
            {
                await handler(currentInner);
            }
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

            var parent = stack.Peek();

            var tab = CurrentTab.Value;
            var currentInner = parent;
            NavigationChanged?.Invoke(tab, currentInner);

            if (_handlers.TryGetValue(tab, out var handler))
            {
                return handler(currentInner);
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
