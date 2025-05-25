namespace Celer.Models
{
    public class DiskInformation
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Label { get; set; }
        public string? Format { get; set; }
        public long Size { get; set; }
        public long AvailableSpace { get; set; }
        public long UsedSpace { get; set; }
        public string? SmartStatus { get; set; }


    }
}
