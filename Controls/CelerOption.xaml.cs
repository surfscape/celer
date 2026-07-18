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
        
        [DependencyProperty]
        public PackIconLucideKind Icon { get; set; } = PackIconLucideKind.Paperclip;

        [DependencyProperty]
        public ImageSource Image { get; set; }

        [DependencyProperty]
        public string Title { get; set; } = "Option title";

        [DependencyProperty]
        public string Description { get; set; }

        [DependencyProperty]
        public object ContentPresenter { get; set; }

        public void OnImageChanged()
        {
            iconEl.Visibility = Visibility.Collapsed;
        }

        public void OnDescriptionChanged(string value)
        {
            if (value == "" || value is null)
                descriptionEl.Visibility = Visibility.Collapsed;
        }

        public CelerOption()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                descriptionEl.Visibility = string.IsNullOrEmpty(Description)
    ? Visibility.Collapsed
    : Visibility.Visible;
            };
        }
    }
}
