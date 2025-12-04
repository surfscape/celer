using Celer.Properties;
using Celer.Services.Energy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace Celer.ViewModels.OptimizationVM
{
    public partial class BatteryViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading = true;

        private BatteryService? batteryService;
        private PowerPlanService? powerPlanService;
        private readonly DispatcherTimer _updateTimer = new()
        {
            Interval = TimeSpan.FromSeconds(2),
        };

        [ObservableProperty]
        private bool hasBattery;

        [ObservableProperty]
        private int batteryPercentage;

        [ObservableProperty]
        private bool isCharging;

        [ObservableProperty]
        private TimeSpan batteryTimeRemaining;

        [ObservableProperty]
        private int batteryHealthPercentage;

        [ObservableProperty]
        private int batteryChargedPercentageHealth;

        [ObservableProperty]
        private ObservableCollection<PowerPlan>? powerPlans;

        [ObservableProperty]
        private PowerPlan? selectedPowerPlan;

        [ObservableProperty]
        private bool isFastBootEnabled;

        [ObservableProperty]
        private int remainingCapacity;

        [ObservableProperty]
        private int factoryCapacity;

        [ObservableProperty]
        private int chargedCapacity;

        [ObservableProperty]
        private int chargedCapacityPercentage;

        [ObservableProperty]
        private bool isLegacyPowerPlansEnabled = MainConfiguration.Default.EnableLegacyPowerPlans;

        public BatteryViewModel()
        {
            _updateTimer.Tick += (_, _) => UpdateBatteryInfo();
        }

        public async Task Initialize()
        {
            bool fastBootDetected = false;
            try
            {
                await Task.Run(() =>
                {
                    batteryService = new BatteryService();
                    if (MainConfiguration.Default.EnableLegacyPowerPlans)
                    {
                        powerPlanService = new PowerPlanService();
                    }

                    fastBootDetected = IsFastStartupEnabled();
                });
                if (powerPlanService != null)
                {
                    PowerPlans = new ObservableCollection<PowerPlan>(powerPlanService.GetAllPowerPlans());
                    SelectedPowerPlan = powerPlanService.GetActivePowerPlan();
                }
                UpdateBatteryInfo();
                IsFastBootEnabled = fastBootDetected;
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception while trying to load BatteryViewModel: " + e.Message);
            }
            finally
            {
                IsLoading = false;
                _updateTimer.Start();
            }
        }

        [RelayCommand]
        private void ApplyPowerPlan()
        {
            if (SelectedPowerPlan != null && powerPlanService != null)
                powerPlanService.SetActivePowerPlan(SelectedPowerPlan.GUID);
        }

        private void UpdateBatteryInfo()
        {
        var info = batteryService?.GetBatteryInfo();
        if (info == null) { 
            HasBattery = false;
            return;
        }
        HasBattery = info.HasBattery;
        if (!HasBattery)
            return;

        BatteryPercentage = info.Percentage;
        IsCharging = info.IsCharging;
        BatteryHealthPercentage = info.Health;
        RemainingCapacity = info.RemainingCapacity;
        BatteryTimeRemaining = info.EstimatedTime;
        BatteryChargedPercentageHealth = info.ChargedCapacity;
        FactoryCapacity = info.FactoryCapacity;
        ChargedCapacity = info.ChargedCapacity;
        ChargedCapacityPercentage = info.ChargedCapacityPercentage;
        }

        partial void OnIsFastBootEnabledChanged(bool value)
        {
            Task.Run(() => SetFastStartup(value));
        }

        private const string RegistryPath =
            @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";
        private const string ValueName = "HiberbootEnabled";

        public static bool IsFastStartupEnabled()
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, false);
            return key != null && Convert.ToInt32(key.GetValue(ValueName, 1)) == 1;
        }

        public static void SetFastStartup(bool enabled)
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, true);
            key?.SetValue(ValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
        }
    }
}
