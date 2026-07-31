using Celer.Interfaces;
using Celer.Models.SystemInfo;
using Celer.Properties;
using Celer.Services.Memory;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace Celer.ViewModels.OptimizationVM
{
    public partial class MemoryViewModel : ObservableObject, INavigationAware
    {
        private readonly MemoryMonitorService _monitorService = new();
        private readonly DispatcherTimer _updateTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(MainConfiguration.Default.GeneralPollingRate),
        };

        [ObservableProperty]
        public partial bool IsLoading { get; set; } = true;

        [ObservableProperty]
        public partial MemoryInfo? Memory { get; set; }

        public ObservableCollection<RamSlotInfo> Slots { get; } = [];

        private void UpdateMemoryInfo(bool continous)
        {
            var mem = _monitorService.GetMemoryInfo();
            Memory = mem;
            if (!continous)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Slots.Clear();
                    foreach (var slot in mem.Slots)
                        Slots.Add(slot);
                });
            }
        }

        public async Task OnNavigatedTo()
        {
            UpdateMemoryInfo(false);
            _updateTimer.Tick += (_, _) => UpdateMemoryInfo(true);
            _updateTimer.Start();
            IsLoading = false;
        }

        public async Task OnNavigatedFrom()
        {
            _updateTimer.Stop();
        }
    }
}
