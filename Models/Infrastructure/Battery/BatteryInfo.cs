namespace Celer.Models.System.Battery
{
    public class BatteryInfo(string Brand, string Model, int SerialNumber, string DeviceID,int FullDesignCapacity)
    {
        public string Brand = Brand;
        public string Model = Model;
        public int SerialNumber = SerialNumber;
        public string BatteryHWID = DeviceID;
        public int FullDesignCapacity = FullDesignCapacity;
    }
}
