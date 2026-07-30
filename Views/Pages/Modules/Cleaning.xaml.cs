using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.Pages.Modules
{
    /// <summary>
    /// Interaction logic for Cleaning.xaml
    /// </summary>
    public partial class Cleaning : UserControl
    {

        public Cleaning()
        {
            InitializeComponent();
            LogListBox.Items.MoveCurrentToLast();
            LogListBox.ScrollIntoView(LogListBox.Items.CurrentItem);
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer scrollViewer &&
                Math.Abs(e.ExtentHeightChange) > 0.0)
            {
                scrollViewer.ScrollToBottom();
            }
        }

    }
}
