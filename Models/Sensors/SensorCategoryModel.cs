using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Celer.Models.Sensors
{
    public partial class SensorCategoryModel : ObservableObject
    {
        public string? Name { get; }
        public MahApps.Metro.IconPacks.PackIconLucideKind Icon { get; }

        public ObservableCollection<SensorItemModel> Sensors { get; } = new();

        [ObservableProperty]
        private bool isExpanded = true;

        public SensorCategoryModel(string name)
        {
            if (name == "Cpu")
            {
                Name = "Processador";
                Icon = MahApps.Metro.IconPacks.PackIconLucideKind.Cpu;
            }
            if (name == "Storage")
            {
                Name = "Armazenamento";
                Icon = MahApps.Metro.IconPacks.PackIconLucideKind.Cylinder;
            }
            else
            {
                Name = name;
            }
        }

        public void AddSensor(LibreHardwareMonitor.Hardware.ISensor sensor)
        {
            Sensors.Add(new SensorItemModel(sensor));
        }

        public void Update()
        {
            foreach (var sensor in Sensors)
                sensor.Update();
        }
    }
}
