namespace Celer.Models.SystemInfo
{
    public class MemoryInfo
    {
        public float UsedMemoryMB { get; set; }
        public double TotalMemoryMB { get; set; }
        public double MemoryUsagePercentage =>
            TotalMemoryMB == 0 ? 0 : (UsedMemoryMB / TotalMemoryMB) * 100;
        public float VirtualUsedMB { get; set; }
        public double VirtualTotalMB { get; set; }
        public double VirtualUsagePercentage =>
            VirtualTotalMB == 0 ? 0 : (VirtualUsedMB / VirtualTotalMB) * 100;
        public float? SpeedMHz { get; set; }
        public List<RamSlotInfo> Slots { get; set; } = new();
    }

    public class RamSlotInfo
    {
        public string SlotNumber { get; set; }
        public bool IsOccupied { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public int SizeMB { get; set; }
        public string MemoryType { get; set; }
        public string FormFactor { get; set; }
        public string SerialNumber { get; set; }
        public string BankLabel { get; set; }
        public string DeviceLocator { get; set; }
    }
}
