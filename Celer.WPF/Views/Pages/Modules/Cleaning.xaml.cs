using System.Windows;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules
{
    /// <summary>
    /// Interaction logic for Cleaning.xaml
    /// </summary>
    public partial class Cleaning : UserControl
    {
        private bool autoScrollPaused = false;
        public Cleaning()
        {
            InitializeComponent();
            LogListBox.Items.MoveCurrentToLast();
            LogListBox.ScrollIntoView(LogListBox.Items.CurrentItem);
            Loaded += (s, e) =>
            {
                LogListBox.SetValue(VirtualizingStackPanel.VirtualizationModeProperty, VirtualizationMode.Recycling);
                LogListBox.Visibility = Visibility.Visible;
            };
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(!autoScrollPaused)
                if (e.OriginalSource is ScrollViewer scrollViewer &&
                    Math.Abs(e.ExtentHeightChange) > 0.0)
                {
                    scrollViewer.ScrollToBottom();
                }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer scrollViewer)
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {
                    autoScrollPaused = false;
                }
                else
                {
                    autoScrollPaused = true;
                }
            }
        }

    }
}
