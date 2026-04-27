namespace Celer.Models.System.Battery
{
    public class BatteryStats(bool IsCharging, bool IsPower, int Percentage, TimeSpan EstimatedRunTime, int RemainingFullCapacity, int RemainingChargeCapacity, int CycleCount, int FullDesignCapacity)
    {
        public bool IsCharging = IsCharging;
        public bool IsPower = IsPower;
        public int Percentage = Percentage;
        public TimeSpan EstimatedRunTime = EstimatedRunTime;
        public int RemainingFullCapacity = RemainingFullCapacity;
        public int RemainingChargeCapacity = RemainingChargeCapacity;
        public int CycleCount = CycleCount;
        public int Heatlh
        {
            get {
                if (FullDesignCapacity == 0) return 0;
                return (RemainingFullCapacity / FullDesignCapacity) * 100;
            }
        }
    }
}
