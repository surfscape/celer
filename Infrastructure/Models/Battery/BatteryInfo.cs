using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Infrastructure.Models.Battery
{
    public class BatteryInfo(bool Availability, string Brand, string Model, int SerialNumber, string DeviceID,int FullDesignCapacity) : ObservableObject
    {
        public bool Available = Availability;
        public string Brand = Brand;
        public string Model = Model;
        public int SerialNumber = SerialNumber;
        public string BatteryHWID = DeviceID;
        public int FullDesignCapacity = FullDesignCapacity;
    }
}
