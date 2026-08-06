using MahApps.Metro.IconPacks;
using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Celer.Controls
{
    /// <summary>
    /// Interaction logic for SettingCategoryButton.xaml
    /// </summary>
    public partial class SettingCategoryButton : UserControl
    {
        [DependencyProperty]
        public PackIconLucideKind Icon { get; set; } = PackIconLucideKind.Bolt;

        [DependencyProperty]
        public string Title { get; set; } = "Option title";

        [DependencyProperty]
        public string Description { get; set; } = "Category description";

        [DependencyProperty]
        public ICommand? Command { get; set; }

        [DependencyProperty]
        public string? CommandParameter { get; set; }
        public SettingCategoryButton()
        {
            InitializeComponent();
        }
    }
}
