namespace Celer.Models.SystemInfo
{
    class SystemInternalModel(double postTime, float cpuUsage, string windowsVersion)
    {
        public double BiosBootUpTime { get; set; } = postTime;
        public float ProcessorUsage { get; set; } = cpuUsage;

        public string WindowsVersion { get; set; } = windowsVersion;
    }
}
