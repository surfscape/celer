using Celer.Services.Energy;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;


namespace Celer.ViewModels.OtimizacaoVM
{
    public partial class BatteryViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading = true;

        private  BatteryService batteryService;
        private  PowerPlanService powerPlanService;
        private readonly DispatcherTimer _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

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
            _updateTimer.Tick += (_, _) => UpdateBatteryInfo();
        }


        public async Task Initialize()
        {
            try
            {
                await Task.Run(() =>
                {
                    batteryService = new BatteryService();
                    powerPlanService = new PowerPlanService();
                    powerPlans = new ObservableCollection<PowerPlan>(powerPlanService.GetAllPowerPlans());
                    selectedPowerPlan = powerPlanService.GetActivePowerPlan();
                    UpdateBatteryInfo();
                    IsFastBootEnabled = IsFastStartupEnabled();
                });
            } catch(Exception e)
            {
                Trace.WriteLine("Erro ao inicializar BatteryViewModel: " + e.Message);
            } finally
            {
                IsLoading = false;
                _updateTimer.Start();
            }
        }

        [RelayCommand]
        private void ApplyPowerPlan()
        {
            if (SelectedPowerPlan != null)
                powerPlanService.SetActivePowerPlan(SelectedPowerPlan.GUID);
        }

        private void UpdateBatteryInfo()
        {
            var info = batteryService.GetBatteryInfo();
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
