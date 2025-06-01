using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Components
{
    public partial class HeadingHelp : UserControl
    {
        public HeadingHelp()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(HeadingHelp), new PropertyMetadata(string.Empty));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register(nameof(HelpText), typeof(string), typeof(HeadingHelp), new PropertyMetadata(string.Empty));

        public string HelpText
        {
            get => (string)GetValue(HelpTextProperty);
            set => SetValue(HelpTextProperty, value);
        }

        public static readonly DependencyProperty LabelStyleProperty =
            DependencyProperty.Register(nameof(LabelStyle), typeof(Style), typeof(HeadingHelp), new PropertyMetadata(null));

        public Style LabelStyle
        {
            get => (Style)GetValue(LabelStyleProperty);
            set => SetValue(LabelStyleProperty, value);
        }

        public static readonly DependencyProperty LabelStyleKeyProperty =
            DependencyProperty.Register(nameof(LabelStyleKey), typeof(string), typeof(HeadingHelp),
                new PropertyMetadata(null, OnLabelStyleKeyChanged));

        public string LabelStyleKey
        {
            get => (string)GetValue(LabelStyleKeyProperty);
            set => SetValue(LabelStyleKeyProperty, value);
        }

        private static void OnLabelStyleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HeadingHelp control && e.NewValue is string key)
            {
                var style = Application.Current.TryFindResource(key) as Style;
                if (style != null)
                {
                    control.LabelStyle = style;
                }
            }
        }
    }
}
