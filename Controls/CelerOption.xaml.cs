using MahApps.Metro.IconPacks;
using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;


namespace Celer.Controls
{
    /// <summary>
    /// Interaction logic for CelerOption.xaml
    /// </summary>
    [ContentProperty(nameof(ContentPresenter))]
    public partial class CelerOption : UserControl
    {
        [DependencyProperty]
        public PackIconLucideKind Icon { get; set; } = PackIconLucideKind.Paperclip;

        [DependencyProperty]
        public string Title { get; set; } = "Option title";


        [DependencyProperty]
        public string? Description { get; set; } = null;

        [DependencyProperty]
        public object ContentPresenter { get; set; }
        public CelerOption()
        {
            InitializeComponent();
        }
    }
}
