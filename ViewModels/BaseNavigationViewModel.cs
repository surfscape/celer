using Celer.Interfaces;
using Celer.Models;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Celer.ViewModels
{
    public abstract partial class BaseNavigationViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly string _navigationKey;

        protected abstract Dictionary<string, NavigationSubView> SubViews { get; }

        [ObservableProperty]
        private NavigationSubView? currentSubView;

        public UserControl? CurrentView => CurrentSubView?.Control;
        public string? CurrentViewName => CurrentSubView?.Name;

        protected BaseNavigationViewModel(NavigationService navigationService, string navigationKey)
        {
            _navigationService = navigationService;
            _navigationKey = navigationKey;
            _navigationService.Register(navigationKey, NavigateTo);
        }

        [RelayCommand]
        public void Navigate(string viewKey) => NavigateTo(viewKey);

        [RelayCommand]
        public void BackToMain() => NavigateTo("Main");

        public void NavigateTo(string viewKey)
        {
            if (CurrentSubView?.Control is INavigationAware previousNav)
                previousNav.OnNavigatedFrom();

            if (string.IsNullOrEmpty(viewKey) || viewKey == "Main")
            {
                CurrentSubView = null;
                return;
            }

            if (SubViews.TryGetValue(viewKey, out var newSubView))
            {
                CurrentSubView = newSubView;
                if (newSubView.Control is INavigationAware newNav)
                    newNav.OnNavigatedTo();
            }
        }

        partial void OnCurrentSubViewChanged(NavigationSubView? value)
        {
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(CurrentViewName));
        }
    }
}
