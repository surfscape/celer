using Celer.Interfaces;
using Celer.Models;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Celer.ViewModels
{
    /// <summary>
    /// Base class for tab viewmodels that host subviews. It is responsible only for switching
    /// the active subview and for that subview's lifecycle; tab level lifecycle is driven by
    /// <see cref="NavigationService"/> through <see cref="INavigationAware"/>.
    /// </summary>
    public abstract partial class BaseNavigationViewModel : ObservableObject, INavigationAware
    {
        private readonly NavigationService _navigationService;
        private readonly NavigationTabKey _navigationKey;
        private readonly IServiceProvider _serviceProvider;

        protected abstract Dictionary<string, SubviewDescriptor> SubViews { get; }

        [ObservableProperty]
        private ObservableObject? currentView;

        private SubviewDescriptor? _currentDescriptor;

        public string? CurrentViewName => _currentDescriptor?.Name;
        public string? CurrentViewDescription => _currentDescriptor?.Description;

        protected BaseNavigationViewModel(NavigationService navigationService, NavigationTabKey navigationKey, IServiceProvider serviceProvider)
        {
            _navigationService = navigationService;
            _navigationKey = navigationKey;
            _serviceProvider = serviceProvider;
            _navigationService.RegisterSubviewHost(navigationKey, NavigateTo);
        }

        [RelayCommand]
        public async Task Navigate(string viewKey) => await _navigationService.Navigate(_navigationKey, viewKey);

        [RelayCommand]
        public async Task BackToMain() => await _navigationService.Navigate(_navigationKey, "Main");

        /// <summary>
        /// Switches the active subview. Invoked by <see cref="NavigationService"/>; a null or
        /// "Main" key clears the subview and shows the tab's root content.
        /// </summary>
        public async Task NavigateTo(string? viewKey)
        {
            await TearDownCurrentViewAsync();

            if (string.IsNullOrEmpty(viewKey) || viewKey == "Main")
                return;

            if (SubViews.TryGetValue(viewKey, out var descriptor))
            {
                var vm = (ObservableObject)_serviceProvider.GetRequiredService(descriptor.ViewModelType);
                _currentDescriptor = descriptor;
                CurrentView = vm;
                OnPropertyChanged(nameof(CurrentViewName));
                OnPropertyChanged(nameof(CurrentViewDescription));

                if (vm is INavigationAware newNav)
                    await newNav.OnNavigatedTo();
            }
        }

        /// <summary>
        /// The tab itself became active. The subview was already restored by
        /// <see cref="NavigateTo"/>, so there is nothing further to do here.
        /// </summary>
        public Task OnNavigatedTo() => Task.CompletedTask;

        /// <summary>
        /// The tab is being left, so release the active subview.
        /// </summary>
        public Task OnNavigatedFrom() => TearDownCurrentViewAsync();

        private async Task TearDownCurrentViewAsync()
        {
            if (CurrentView is null)
                return;

            if (CurrentView is INavigationAware previousNav)
                await previousNav.OnNavigatedFrom();

            if (CurrentView is IDisposable disposable)
                disposable.Dispose();

            _currentDescriptor = null;
            CurrentView = null;
            OnPropertyChanged(nameof(CurrentViewName));
            OnPropertyChanged(nameof(CurrentViewDescription));
        }
    }
}
