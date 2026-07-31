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
    public abstract partial class BaseNavigationViewModel : ObservableObject
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
            _navigationService.Register(navigationKey, NavigateTo);
        }

        [RelayCommand]
        public async Task Navigate(string viewKey) => await _navigationService.Navigate(_navigationKey, viewKey);

        [RelayCommand]
        public async Task BackToMain() => await _navigationService.Navigate(_navigationKey, "Main");

        public async Task NavigateTo(string viewKey)
        {
            if (currentView is INavigationAware previousNav)
            {
                await previousNav.OnNavigatedFrom();
            }

            if (currentView is IDisposable d)
                d.Dispose();

            if (string.IsNullOrEmpty(viewKey) || viewKey == "Main")
            {
                _currentDescriptor = null;
                CurrentView = null;
                OnPropertyChanged(nameof(CurrentViewName));
                OnPropertyChanged(nameof(CurrentViewDescription));
                return;
            }

            if (SubViews.TryGetValue(viewKey, out var descriptor))
            {
                var vm = (ObservableObject?)_serviceProvider.GetService(descriptor.ViewModelType) ?? (ObservableObject?)_serviceProvider.GetRequiredService(descriptor.ViewModelType);
                _currentDescriptor = descriptor;
                CurrentView = vm;
                OnPropertyChanged(nameof(CurrentViewName));
                OnPropertyChanged(nameof(CurrentViewDescription));

                if (vm is INavigationAware newNav)
                    await newNav.OnNavigatedTo();
            }
        }
    }
}
