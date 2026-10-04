using Celer.Interfaces;
using Celer.Models;

namespace Celer.Services
{
	/// <summary>
	/// Singleton service that manages navigation between tabs and corresponding subviews. It tracks the current history stack, registered tab viewmodels and views, and provides lifecycle callbacks to viewmodels implementing <see cref="INavigationAware"/>.
	/// </summary>
	public class NavigationService
	{
		public const string RootView = "Main";
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

		private NavigationTabKey? _currentTab;
		private string? _activeSubview;
		public NavigationTabKey? CurrentTab => _currentTab;

		/// <summary>
		/// Registers a tab's viewmodel so it can take part in the navigation lifecycle.
		/// </summary>
		public void RegisterTab(NavigationTabKey key, object viewModel, Func<string?, Task>? handler = null)
		{
			_tabViewModels[key] = viewModel;
			if (!_tabStacks.ContainsKey(key))
				_tabStacks[key] = new Stack<string?>([null]);
			if (handler is not null)
				_subviewHosts[key] = handler;
		}

		public string? CurrentSubview => _currentTab == null ? null : PeekOrNull(_currentTab.Value);

		public event Action<NavigationTabKey?>? NavigationChanged;

		public bool CanGoBack
		{
			get
			{
				var subview = _currentTab == null ? null : PeekOrNull(_currentTab.Value);
				return !string.IsNullOrEmpty(subview) && !string.Equals(subview, RootView, StringComparison.Ordinal);
			}
		}

		public async Task Navigate(NavigationTabKey tabKey, string? subviewName = null)
		{
			string? targetSubview = string.IsNullOrEmpty(subviewName) || subviewName == RootView ? null : subviewName;

			if (_currentTab == tabKey && _activeSubview == targetSubview)
				return;

			if (!_tabStacks.TryGetValue(tabKey, out var stack))
				_tabStacks[tabKey] = stack = new Stack<string?>([null]);

			if (targetSubview is null)
			{
				stack.Clear();
				stack.Push(null);
			}
			else if (stack.Count == 0 || stack.Peek() != targetSubview)
			{
				stack.Push(targetSubview);
			}

			if (_currentTab is { } previousTab && previousTab != tabKey)
				await NotifyNavigatedFrom(previousTab);

			bool tabChanged = _currentTab != tabKey;

			_currentTab = tabKey;
			_activeSubview = stack.Count > 0 ? stack.Peek() : null;

			NavigationChanged?.Invoke(_currentTab);

			if (_subviewHosts.TryGetValue(tabKey, out var host))
				await host(_activeSubview);

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

		public async Task BackToParent()
		{
			if (_currentTab == null)
				return;

			if (!CanGoBack)
				return;

			var stack = _tabStacks[_currentTab.Value];
			if (stack.Count > 1)
				stack.Pop();

			var tab = _currentTab.Value;
			var currentSubview = PeekOrNull(tab);

			_activeSubview = currentSubview;

			NavigationChanged?.Invoke(tab);

			if (_subviewHosts.TryGetValue(tab, out var host))
			{
				await host(currentSubview);
			}
		}
		public string? GetSubviewForTab(NavigationTabKey tabKey) => PeekOrNull(tabKey);

		private string? PeekOrNull(NavigationTabKey key)
		{
			if (!_tabStacks.TryGetValue(key, out var stack) || stack.Count == 0)
				return null;
			return stack.Peek();
		}
	}
}
