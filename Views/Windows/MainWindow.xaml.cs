using Celer.Properties;
using Celer.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace Celer.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        IsVisibleChanged += new DependencyPropertyChangedEventHandler(CheckVisibility);
        DataContext = _viewModel;
        QCMenu.DataContext = new QCMenuViewModel();
    }

    void CheckVisibility(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Visibility == Visibility.Visible && DataContext == null)
            DataContext = _viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (MainConfiguration.Default.CloseShouldMinimize)
        {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
            DataContext = null;
        }
        else
            e.Cancel = false;
    }
}
