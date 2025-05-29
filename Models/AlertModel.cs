namespace Celer.Models
{
    public enum AlertType
    {
        CPU,
        Memory,
        Process
    }
    public class AlertModel
    {
        public AlertType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public MahApps.Metro.IconPacks.PackIconLucideKind Icon { get; set; }
    }
}
