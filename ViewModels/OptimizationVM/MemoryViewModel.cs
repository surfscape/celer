using Celer.Models.SystemInfo;
using Celer.Services.Memory;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace Celer.ViewModels.OptimizationVM
{
    public partial class MemoryViewModel : ObservableObject
    {
        private readonly MemoryMonitorService _monitorService = new();
        private readonly DispatcherTimer _updateTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1),
        };

        [ObservableProperty]
        private MemoryInfo? memory;

        public ObservableCollection<RamSlotInfo> Slots { get; } = [];

        public MemoryViewModel()
        { 
            UpdateMemoryInfo(false);
            _updateTimer.Tick += (_, _) => UpdateMemoryInfo(true);
            _updateTimer.Start();
        }

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
    }
}
