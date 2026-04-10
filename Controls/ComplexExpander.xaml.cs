using MahApps.Metro.IconPacks;
using Metalama.Patterns.Wpf;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public string Description { get; set; } = "Expander description";

        [DependencyProperty]
        public object ExpanderContent { get; set; }

        public ComplexExpander()
        {
            InitializeComponent();
        }
    }
}
