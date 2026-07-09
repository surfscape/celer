using ByteSizeLib;
using Celer.Properties;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Threading;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsAdvancedViewModel : SettingsBaseViewModel
    {
        private readonly DispatcherTimer _timer;

        public static Dictionary<string, int> PollingOptions { get; } = new()
        {
            { "250ms", 250 },
            { "500ms", 500 },
            { "1000ms (Default)", 1000 },
            { "1500ms", 1500 }
        };

        [ObservableProperty]
        public partial KeyValuePair<string, int> PoolingRate { get; set; } = PollingOptions.FirstOrDefault(x => x.Value == MainConfiguration.Default.GeneralPollingRate);

        partial void OnPoolingRateChanged(KeyValuePair<string, int> value)
        {
            MainConfiguration.Default.GeneralPollingRate = value.Value;
            MainConfiguration.Default.Save();
        }

        public ObservableCollection<string> RenderingModes { get; } = ["Auto (Default)", "Hardware", "Software only" ];

        [ObservableProperty]
        public partial int GraphicRenderingMode { get; set; } = MainConfiguration.Default.GraphicRenderingMode;

        partial void OnGraphicRenderingModeChanged(int value)
        {
            MainConfiguration.Default.GraphicRenderingMode = value;
            MainConfiguration.Default.Save();
        }

        /* Debug Information Properties */
        [ObservableProperty]
        public partial string ProcessMemory { get; set; } = "0.00MB";

        [ObservableProperty]
        public partial string BuildType { get; set; } = "Unknown";

        private readonly SettingsNavigation _settingsNavigation;


        public SettingsAdvancedViewModel(SettingsNavigation settingsNavigation)
        {
            _settingsNavigation = settingsNavigation;
            _settingsNavigation.PageTitle = "Advanced";
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateDebugInfo();
            _timer.Start();
            // Source - https://stackoverflow.com/a/60545278
            // Posted by Egor Novikov
            // Retrieved 2026-07-06, License - CC BY-SA 4.0
            var assemblyConfigurationAttribute = typeof(SettingsAdvancedViewModel).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            if(assemblyConfigurationAttribute is not null && assemblyConfigurationAttribute.Configuration is not null)
                BuildType = assemblyConfigurationAttribute.Configuration;

        }

        private void UpdateDebugInfo()
        {
            ProcessMemory = ByteSize.FromBytes(GC.GetTotalMemory(true)).ToString();
        }
    }
}
