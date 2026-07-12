using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Infrastructure.Models.Battery
{
    public partial class BatteryStats : ObservableObject
    {
        [ObservableProperty]
        private bool isCharging = false;

        [ObservableProperty]
        private bool isPower = false;

        [ObservableProperty]
        private int percentage = 0;

        [ObservableProperty]
        private TimeSpan estimatedRunTime = TimeSpan.Zero;

        [ObservableProperty]
        private int remainingFullCapacity = 0;

        [ObservableProperty]
        private int remainingChargeCapacity = 0;

        [ObservableProperty]
        private int cycleCount = 0;

        [ObservableProperty]
        private int fullDesignCapacity = 0;

        [ObservableProperty]
        private int health = 0;

        [ObservableProperty]
        private int remainingChargeCapacityPercentage = 0;

        public BatteryStats(bool isCharging, bool isPower, int percentage, TimeSpan estimatedRunTime,
                            int remainingFullCapacity, int remainingChargeCapacity, int cycleCount, int fullDesignCapacity)
        {
            IsCharging = isCharging;
            IsPower = isPower;
            Percentage = percentage;
            EstimatedRunTime = estimatedRunTime;
            RemainingFullCapacity = remainingFullCapacity;
            RemainingChargeCapacity = remainingChargeCapacity;
            CycleCount = cycleCount;
            FullDesignCapacity = fullDesignCapacity;
            Health = (RemainingFullCapacity * 100) / FullDesignCapacity;
            RemainingChargeCapacityPercentage = (int)((double)RemainingChargeCapacity / FullDesignCapacity * 100);
        }
    }
}