using MahApps.Metro.IconPacks;
using Metalama.Patterns.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;


namespace Celer.Controls
{
    /// <summary>
    /// Interaction logic for CelerOption.xaml
    /// </summary>
    [ContentProperty(nameof(ContentPresenter))]
    public partial class CelerOption : UserControl
    {
        public CelerOption() => InitializeComponent();

        [DependencyProperty]
        public PackIconLucideKind Icon { get; set; } = PackIconLucideKind.Paperclip;

        [DependencyProperty]
        public ImageSource Image { get; set; }

        [DependencyProperty]
        public string Title { get; set; } = "Option title";

        [DependencyProperty]
        public string Description { get; set; } = "Description";

        [DependencyProperty]
        public object ContentPresenter { get; set; }


        public void OnImageChanged()
        {
            iconEl.Visibility = Visibility.Collapsed;
        }
    }
}
