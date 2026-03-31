using Celer.Properties;
using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace Celer.Utilities
{
    public class CompactTabControl : TabControl
    {
        [DependencyProperty]
        public bool CompactMode { get; set; } = MainConfiguration.Default.SidebarCompactMode;

        [DependencyProperty]
        public string UserContentSize { get; set; } = MainConfiguration.Default.AppearanceMaxWidth ? "960" : "auto";

        [DependencyProperty]
        public string Scroll { get; set; } = "auto";

    }


}
