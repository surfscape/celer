using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models.Sensors
{

    public partial class SensorItemViewModel : ObservableObject
    {
        public string Name { get; }

        [ObservableProperty]
        private string? value;

        private readonly LibreHardwareMonitor.Hardware.ISensor _sensor;

        public SensorItemViewModel(LibreHardwareMonitor.Hardware.ISensor sensor)
        {
            _sensor = sensor;
            Name = sensor.Name;
            Update();
        }

        public void Update()
        {
            Value = _sensor.Value.HasValue ? $"{_sensor.Value.Value:F1} {_sensor.SensorType switch
            {
                LibreHardwareMonitor.Hardware.SensorType.Temperature => "°C",
                LibreHardwareMonitor.Hardware.SensorType.Fan => "RPM",
                _ => ""
            }}" : "N/A";
        }
    }

}
