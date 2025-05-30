using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Celer.Models.Sensors
{
    public partial class SensorCategoryViewModel : ObservableObject
    {
        public string? Name { get; }
        public MahApps.Metro.IconPacks.PackIconLucideKind Icon { get; }

        public ObservableCollection<SensorItemViewModel> Sensors { get; } = new();

        [ObservableProperty]
        private bool isExpanded = true;

        public SensorCategoryViewModel(string name)
        {
            if(name == "Cpu")
            {
                Name = "Processador";
                Icon = MahApps.Metro.IconPacks.PackIconLucideKind.Cpu;
            }
            if(name == "Storage")
            {
                Name = "Armazenamento";
                Icon = MahApps.Metro.IconPacks.PackIconLucideKind.Cylinder;
            }
        }

        public void AddSensor(LibreHardwareMonitor.Hardware.ISensor sensor)
        {
            Sensors.Add(new SensorItemViewModel(sensor));
        }

        public void Update()
        {
            foreach (var sensor in Sensors)
                sensor.Update();
        }
    }
}
