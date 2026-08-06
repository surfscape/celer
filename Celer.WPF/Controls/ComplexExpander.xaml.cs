using MahApps.Metro.IconPacks;
using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Celer.Controls
{
    /// <summary>
    /// Interaction logic for ComplexExpander.xaml
    /// </summary>
    [ContentProperty(nameof(ExpanderContent))]
    public partial class ComplexExpander : UserControl
    {
        [DependencyProperty]
        public PackIconLucideKind Icon { get; set; } = PackIconLucideKind.Paperclip;

        [DependencyProperty]
        public string Title { get; set; } = "Expander title";


        [DependencyProperty]
        public string? Description { get; set; } = null;

        [DependencyProperty]
        public object ExpanderContent { get; set; }

        public ComplexExpander()
        {
            InitializeComponent();
        }
    }
}
