using Celer.Infrastructure;
using Celer.Infrastructure.Models.Battery;
using Celer.Properties;
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
        public partial bool IsLoading { get; set; } = true;

        private bool hasBattery = false;
        private readonly Services.Energy.PowerPlanService? powerPlanService;
        private readonly Battery? batteryService = new();
        private readonly DispatcherTimer _updateTimer = new()
        {
            Interval = TimeSpan.FromSeconds(2),
        };

        [ObservableProperty]
        public partial BatteryInfo? BatteryStaticData { get; set; }
        [ObservableProperty]
        public partial BatteryStats? BatteryStats { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Services.Energy.PowerPlan>? PowerPlans { get; set; }
        [ObservableProperty]
        public partial Services.Energy.PowerPlan? SelectedPowerPlan { get; set; }

        [ObservableProperty]
        public partial bool IsFastBootEnabled { get; set; }

        [ObservableProperty]
        public partial bool IsHibernationEnabled { get; set; } = GetHibernationStatus();
        [ObservableProperty]
        public partial bool IsLegacyPowerPlansEnabled { get; set; } = MainConfiguration.Default.EnableLegacyPowerPlans;

        public BatteryViewModel()
        {
            if (MainConfiguration.Default.EnableLegacyPowerPlans)
                powerPlanService = new Services.Energy.PowerPlanService();
            _updateTimer.Tick += (_, _) => UpdateBatteryInfo();
        }

        public async Task Initialize()
        {
            bool fastBootDetected = false;
            try
            {
                await Task.Run(() =>
                {
                    if (batteryService is not null)
                    {
                        hasBattery = true;
                        BatteryStaticData = batteryService.BatteryStaticData;
                    }
                    fastBootDetected = IsFastStartupEnabled();
                });
                if (powerPlanService is not null)
                {
                    PowerPlans = new ObservableCollection<Services.Energy.PowerPlan>(powerPlanService.GetAllPowerPlans());
                    SelectedPowerPlan = powerPlanService.GetActivePowerPlan();
                }
                UpdateBatteryInfo();
                IsFastBootEnabled = fastBootDetected;
            }
            catch (ArgumentNullException e)
            {
                Debug.WriteLine($"ArgumentNullExpection failed to load BatteryViewModel: ${e.Message}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception while trying to initialize BatteryViewModel: ${e.Message}");
            }
            finally
            {
                IsLoading = false;
                if (hasBattery) _updateTimer.Start();
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
            if (batteryService is not null) { 
                batteryService.Update();
                BatteryStats = batteryService.BatteryStats;
        }
        }


        private const string RegistryPath =
            @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";
        private const string ValueName = "HiberbootEnabled";

        public static bool IsFastStartupEnabled()
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, false);
            return key != null && Convert.ToInt32(key.GetValue(ValueName, 1)) == 1;
        }

        partial void OnIsFastBootEnabledChanged(bool value)
        {
            Task.Run(() => SetFastStartup(value));
        }
        public static void SetFastStartup(bool enabled)
        {
            using var key = Registry.LocalMachine.OpenSubKey(RegistryPath, true);
            key?.SetValue(ValueName, enabled ? 1 : 0, RegistryValueKind.DWord);
        }

        public static bool GetHibernationStatus()
        {
            // TODO: Implement Hibernation
            return true;
        }
    }
}
