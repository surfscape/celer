using Celer.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Celer.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{


    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
    }
}
