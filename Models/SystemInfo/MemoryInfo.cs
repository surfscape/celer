namespace Celer.Models.SystemInfo
{
    public class MemoryInfo
    {
        public float UsedMemoryMB { get; set; }
        public double TotalMemoryMB { get; set; }
        public double VirtualUsedMB { get; set; }
        public double VirtualTotalMB { get; set; }
        public float? SpeedMHz { get; set; }
        public List<RamSlotInfo> Slots { get; set; } = [];
    }

    public class RamSlotInfo
    {
        public string SlotNumber { get; set; } = String.Empty;
        public bool IsOccupied { get; set; } = false;
        public string Manufacturer { get; set; } = String.Empty;
        public string Model { get; set; } = String.Empty;
        public int SizeMB { get; set; }
        public string MemoryType { get; set; } = String.Empty;
        public string FormFactor { get; set; } = String.Empty;
        public string BankLabel { get; set; } = String.Empty;
        public string DeviceLocator { get; set; } = String.Empty;
    }
}
