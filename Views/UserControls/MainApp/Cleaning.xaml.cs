using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Cleaning.xaml
    /// </summary>
    public partial class Cleaning : UserControl
    {
        private readonly CleanEngine _viewModel;

        public Cleaning(CleanEngine viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
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
