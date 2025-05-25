using Celer.ViewModels;
using System.Windows;

namespace Celer.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

    }


}

public class TabHeaderData
{
    public string? Title { get; set; }
    public MahApps.Metro.IconPacks.PackIconLucideKind Icon { get; set; }
}
