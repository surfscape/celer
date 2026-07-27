using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace Celer.Views.Windows
{
    /// <summary>
    /// Interaction logic for QuickCenter.xaml
    /// </summary>
    // TODO: QuickCenter Window code is pretty ugly and was cobbled up together from various StackOverflow answers but I have the intention to cleaned it up
    public partial class QuickCenter : Window
    {
        // Source - https://stackoverflow.com/a/958980
        // Posted by Joe White, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-07-18, License - CC BY-SA 4.0

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        public QuickCenter()
        {
            InitializeComponent();
            WindowChrome.SetWindowChrome(
            this,
            new WindowChrome
            {
                CaptionHeight = 48,
                CornerRadius = new CornerRadius(12),
                GlassFrameThickness = new Thickness(-1),
                UseAeroCaptionButtons = true,
                NonClientFrameEdges = GetPrefferedNonClientFrameEdges()
            });

            StateChanged += (s, e) => UpdateMainWindowVisuals();
            Activated += (s, e) => UpdateMainWindowVisuals();
            Deactivated += (s, e) => Close();
            Loaded += (s, e) => HideCloseButton();
        }

        private void HideCloseButton()
        {
            // Source - https://stackoverflow.com/a/958980
            // Posted by Joe White, modified by community. See post 'Timeline' for change history
            // Retrieved 2026-07-18, License - CC BY-SA 4.0

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }


        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void UpdateMainWindowVisuals()
        {
            MainGrid.Margin = default;


            if (SystemParameters.HighContrast == true)
            {
                HighContrastBorder.SetResourceReference(BorderBrushProperty, IsActive ? SystemColors.ActiveCaptionBrushKey : SystemColors.InactiveCaptionBrushKey);
            }
            else
            {
                HighContrastBorder.BorderBrush = Brushes.Transparent;
                HighContrastBorder.BorderThickness = new Thickness(0);
            }

            if (IsWindows11OrGreater())
            {
                WindowChrome wc = WindowChrome.GetWindowChrome(this);
                wc?.NonClientFrameEdges = GetPrefferedNonClientFrameEdges();
            }
        }

        private static NonClientFrameEdges GetPrefferedNonClientFrameEdges()
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            WindowInteropHelper helper = new(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
            PositionWindowAtBottomRight();
        }
        private void PositionWindowAtBottomRight()
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width - 12;
            Top = desktopWorkingArea.Bottom - Height - 12;
        }
    }
}
