namespace Celer.Models.SystemInfo
{
    class SystemInternalModel
    {
        public double BiosBootUpTime { get; set; }
        public float ProcessorUsage { get; set; }

        public string WindowsVersion { get; set; }
        public SystemInternalModel(double postTime, float cpuUsage, string windowsVersion)
        {
            BiosBootUpTime = postTime;
            ProcessorUsage = cpuUsage;
            WindowsVersion = windowsVersion;
        }
    }
}
