using Celer.Models.Sensors;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Celer.ViewModels.SubViews
{
    public partial class SensorViewModel : ObservableObject
    {
        public ObservableCollection<SensorCategoryViewModel> Categories { get; } = [];

        private Computer _computer;

        private readonly DispatcherTimer _updateTimer;

        public SensorViewModel()
        {
            _computer = new()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true,
                IsMemoryEnabled = true
            };
            _computer.Open();
            LoadSensors();

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            _updateTimer.Tick += (_, _) => Update();
            _updateTimer.Start();
        }

        private void LoadSensors()
        {
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                var category = new SensorCategoryViewModel(hardware.HardwareType.ToString());

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType is SensorType.Temperature or SensorType.Fan)
                        category.AddSensor(sensor);
                }

                if (category.Sensors.Count > 0)
                    Categories.Add(category);
            }
        }

        private void Update()
        {
            foreach (var hardware in _computer.Hardware)
                hardware.Update();

            foreach (var category in Categories)
                category.Update();
        }

    }
}
