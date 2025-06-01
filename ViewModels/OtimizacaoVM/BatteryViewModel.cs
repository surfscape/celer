using Celer.Services.Energy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows.Threading;


namespace Celer.ViewModels.OtimizacaoVM
{
    public partial class BatteryViewModel : ObservableObject
    {
        private readonly BatteryService _batteryService;
        private readonly PowerPlanService _powerPlanService;
        private readonly DispatcherTimer _updateTimer;

        [ObservableProperty] private bool hasBattery;
        [ObservableProperty] private int batteryPercentage;
        [ObservableProperty] private bool isCharging;
        [ObservableProperty] private TimeSpan batteryTimeRemaining;
        [ObservableProperty] private int batteryHealthPercentage;

        [ObservableProperty] private ObservableCollection<PowerPlan> powerPlans;
        [ObservableProperty] private PowerPlan? selectedPowerPlan;

        [ObservableProperty] private bool isFastBootEnabled;

        public BatteryViewModel()
        {
            _batteryService = new BatteryService();
            _powerPlanService = new PowerPlanService();

            powerPlans = new ObservableCollection<PowerPlan>(_powerPlanService.GetAllPowerPlans());
            selectedPowerPlan = _powerPlanService.GetActivePowerPlan();

            UpdateBatteryInfo();

            IsFastBootEnabled = IsFastStartupEnabled();

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _updateTimer.Tick += (_, _) => UpdateBatteryInfo();
            _updateTimer.Start();
        }

        [RelayCommand]
        private void ApplyPowerPlan()
        {
            if (SelectedPowerPlan != null)
                _powerPlanService.SetActivePowerPlan(SelectedPowerPlan.GUID);
        }

        private void UpdateBatteryInfo()
        {
            var info = _batteryService.GetBatteryInfo();
            HasBattery = info.HasBattery;
            if (!HasBattery) return;

            BatteryPercentage = info.Percentage;
            IsCharging = info.IsCharging;
            BatteryHealthPercentage = info.Health;
            BatteryTimeRemaining = info.EstimatedTime;
        }
        partial void OnIsFastBootEnabledChanged(bool value)
        {
            SetFastStartup(value);
        }

        private const string RegistryPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";
        private const string ValueName = "HiberbootEnabled";

        public static bool IsFastStartupEnabled()
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, false);
            return key != null && Convert.ToInt32(key.GetValue(ValueName, 1)) == 1;
        }

        public static void SetFastStartup(bool enabled)
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, true);
            if (key != null)
            {
                key.SetValue(ValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
            }
        }
    }
}
