using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Limpeza.xaml
    /// </summary>
    public partial class Limpeza : UserControl
    {
        private readonly CleanEngine _viewModel;

        public Limpeza(CleanEngine viewModel)
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
