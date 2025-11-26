using System.Windows;
using System.Windows.Controls;


namespace Celer.Views.Components
{
    /// <summary>
    /// Interaction logic for ModuleHeader.xaml
    /// </summary>
    public partial class ModuleHeader : UserControl
    {
        public ModuleHeader()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CurrentViewProperty =
                 DependencyProperty.Register(
                     nameof(CurrentView),
                     typeof(UserControl),
                     typeof(ModuleHeader),
                     new PropertyMetadata(null)
                 );

        public UserControl CurrentView
        {
            get => (UserControl)GetValue(CurrentViewProperty);
            set => SetValue(CurrentViewProperty, value);
        }


        public static readonly DependencyProperty ViewLabelProperty = DependencyProperty.Register(
    nameof(ViewLabel),
    typeof(string),
    typeof(ModuleHeader),
    new PropertyMetadata(string.Empty)
);

        public string ViewLabel
        {
            get => (string)GetValue(ViewLabelProperty);
            set => SetValue(ViewLabelProperty, value);
        }

        public static readonly DependencyProperty ViewDescriptionProperty = DependencyProperty.Register(
nameof(ViewDescription),
typeof(string),
typeof(ModuleHeader),
new PropertyMetadata(string.Empty)
);

        public string ViewDescription
        {
            get => (string)GetValue(ViewDescriptionProperty);
            set => SetValue(ViewDescriptionProperty, value);
        }
    }
}
