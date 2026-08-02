using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Celer.Views.Components
{
    /// <summary>
    /// Interaction logic for ModuleButton.xaml
    /// </summary>
    public partial class ModuleButton : UserControl
    {
        public ModuleButton()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty NavigateCommandProperty =
                   DependencyProperty.Register(
                       nameof(NavigateCommand),
                       typeof(ICommand),
                       typeof(ModuleButton),
                       new PropertyMetadata(null)
                   );

        public ICommand NavigateCommand
        {
            get => (ICommand)GetValue(NavigateCommandProperty);
            set => SetValue(NavigateCommandProperty, value);
        }


        public static readonly DependencyProperty NavigateToProperty =
            DependencyProperty.Register(
                nameof(NavigateTo),
                typeof(object),
                typeof(ModuleButton),
                new PropertyMetadata(null)
            );

        public object NavigateTo
        {
            get => GetValue(NavigateToProperty);
            set => SetValue(NavigateToProperty, value);
        }


        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
    nameof(Label),
    typeof(string),
    typeof(ModuleButton),
    new PropertyMetadata(string.Empty)
);

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }


        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
    nameof(Description),
    typeof(string),
    typeof(ModuleButton),
    new PropertyMetadata(string.Empty)
);

        public string Description
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }




        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(MahApps.Metro.IconPacks.PackIconLucideKind),
            typeof(ModuleButton),
            new PropertyMetadata(MahApps.Metro.IconPacks.PackIconLucideKind.Cuboid));


        public MahApps.Metro.IconPacks.PackIconLucideKind Icon
        {
            get => (MahApps.Metro.IconPacks.PackIconLucideKind)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

    }
}
