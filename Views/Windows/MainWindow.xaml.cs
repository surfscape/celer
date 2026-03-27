using Celer.Properties;
using Celer.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Celer.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
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
    private void ButtonMinimizeOnClick(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void ButtonMaximizeOnClick(object sender, RoutedEventArgs e)
    {
        SystemCommands.MaximizeWindow(this);
    }

    private void ButtonRestoreOnClick(object sender, RoutedEventArgs e)
    {
        SystemCommands.RestoreWindow(this);
    }

    private void RestoreOrMaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }
        else
        {
            WindowState = WindowState.Maximized;
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
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
