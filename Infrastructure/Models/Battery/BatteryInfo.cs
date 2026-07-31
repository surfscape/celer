using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Infrastructure.Models.Battery
{
    public class BatteryInfo(bool Availability, string Brand, string Model, int SerialNumber, string DeviceID, int FullDesignCapacity) : ObservableObject
    {
        public bool Available { get; set; } = Availability;
        public string Brand { get; set; } = Brand;
        public string Model { get; set; } = Model;
        public int SerialNumber { get; set; } = SerialNumber;
        public string BatteryHWID { get; set; } = DeviceID;
        public int FullDesignCapacity { get; set; } = FullDesignCapacity;
    }
}
