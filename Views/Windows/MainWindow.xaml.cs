using Celer.Properties;
using Celer.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;


namespace Celer.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    public MainWindow(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        IsVisibleChanged += new DependencyPropertyChangedEventHandler(CheckVisibility);
        InitializeComponent();


        WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 48,
                CornerRadius = new CornerRadius(12),
                GlassFrameThickness = new Thickness(-1),
                ResizeBorderThickness = ResizeMode == ResizeMode.NoResize ? default : new Thickness(4),
                UseAeroCaptionButtons = true,
                NonClientFrameEdges = GetPrefferedNonClientFrameEdges()
            }
        );

        StateChanged += (s, e) => UpdateMainWindowVisuals();
        Activated += (s, e) => UpdateMainWindowVisuals();
        Deactivated += (s, e) => UpdateMainWindowVisuals();

        DataContext = _viewModel;
        QCMenu.DataContext = new QCMenuViewModel();
    }


    private void UpdateMainWindowVisuals()
    {
        MainGrid.Margin = default;
        if (WindowState == WindowState.Maximized)
        {
            MainGrid.Margin = SystemParameters.HighContrast ? new Thickness(0, 8, 0, 0) : new Thickness(8);
        }

        UpdateTitleBarButtonsVisibility();

        if (SystemParameters.HighContrast == true)
        {
            HighContrastBorder.SetResourceReference(BorderBrushProperty, IsActive ? SystemColors.ActiveCaptionBrushKey :
                                                                                    SystemColors.InactiveCaptionBrushKey);
            HighContrastBorder.BorderThickness = new Thickness(8, 1, 8, 8);
        }
        else
        {
            HighContrastBorder.BorderBrush = Brushes.Transparent;
            HighContrastBorder.BorderThickness = new Thickness(0);
        }

        if (IsWindows11OrGreater())
        {
            WindowChrome wc = WindowChrome.GetWindowChrome(this);
            if (wc is not null)
                wc.NonClientFrameEdges = GetPrefferedNonClientFrameEdges(); ;
        }
    }

    private void UpdateTitleBarButtonsVisibility()
    {
        if (IsBackdropDisabled() || !IsBackdropSupported() ||
                SystemParameters.HighContrast == true)
        {
            MinimizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Visible;
            CloseButton.Visibility = Visibility.Visible;
        }
        else
        {
            MinimizeButton.Visibility = Visibility.Collapsed;
            MaximizeButton.Visibility = Visibility.Collapsed;
            CloseButton.Visibility = Visibility.Collapsed;
        }
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

    private NonClientFrameEdges GetPrefferedNonClientFrameEdges()
    {
        if (SystemParameters.HighContrast == true || IsWindows11OrGreater() == false)
            return NonClientFrameEdges.None;

        return NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left;
    }

    public static bool IsBackdropDisabled()
    {
        var appContextBackdropData = AppContext.GetData("Switch.System.Windows.Appearance.DisableFluentThemeWindowBackdrop");
        bool disableFluentThemeWindowBackdrop = false;

        if (appContextBackdropData != null)
        {
            disableFluentThemeWindowBackdrop = bool.Parse(Convert.ToString(appContextBackdropData));
        }

        return disableFluentThemeWindowBackdrop;
    }


    public static bool IsBackdropSupported()
    {
        var os = Environment.OSVersion;
        var version = os.Version;

        return version.Major >= 10 && version.Build >= 22621;
    }

    public static bool IsWindows11OrGreater()
    {
        var os = Environment.OSVersion;
        var version = os.Version;

        return (version.Major >= 10 && version.Build >= 22000);
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



