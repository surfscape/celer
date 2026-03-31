using ByteSizeLib;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsAdvancedViewModel : SettingsBaseViewModel
    {
        [ObservableProperty]
        private string processMemory;

        private readonly DispatcherTimer _timer;
        public SettingsAdvancedViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += new EventHandler(updateDebugInfo);
            _timer.Start();
        }

        private void updateDebugInfo(object sender, EventArgs e)
        {
            ProcessMemory = ByteSize.FromBytes(GC.GetTotalMemory(true)).ToString();
        }
    }
}
