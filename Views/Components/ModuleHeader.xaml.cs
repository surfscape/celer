using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public static readonly DependencyProperty NavigateCommandProperty =
           DependencyProperty.Register(
               nameof(NavigateCommand),
               typeof(ICommand),
               typeof(ModuleHeader),
               new PropertyMetadata(null)
           );

        public ICommand NavigateCommand
        {
            get => (ICommand)GetValue(NavigateCommandProperty);
            set => SetValue(NavigateCommandProperty, value);
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
}
}
