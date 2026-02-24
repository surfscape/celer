using Celer.Models;
using Celer.Properties;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Celer.Views.Windows.Utils
{
    /// <summary>
    /// Interaction logic for SurfScapeGateway.xaml
    /// </summary>
    public partial class SurfScapeGateway : Window
    {

        // Source - https://stackoverflow.com/a
        // Posted by Joe White, modified by community.
        // Retrieved 2025-11-12, License - CC BY-SA 4.0

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [LibraryImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        private static partial int GetWindowLong(IntPtr hWnd, int nIndex);
        [LibraryImport("user32.dll", EntryPoint = "SetWindowLongA")]
        private static partial int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private readonly SurfScapeGatewayViewModel _viewModel;
        private readonly MainWindow _mainWindow;

        /// <summary>
        /// Used to determine whether the window was triggered on startup or not. This is to make sure that if the user has disabled auto updates, it can still open if triggered manually.
        /// </summary>
        public bool MainWindowTrigger { get; set; } = false;
        public bool SilentStartup { get; set; } = false;

        public SurfScapeGateway(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _viewModel = new SurfScapeGatewayViewModel(MainWindowTrigger) { IsDone = InitializeApp };
            DataContext = _viewModel;
            Loaded += SurfScapeGateway_Loaded;
        }

        private void InitializeApp()
        {
            if (MainWindowTrigger)
            {
                if (SilentStartup)
                    _mainWindow.Visibility = Visibility.Collapsed;
                else
                    _mainWindow.Show();
                Close();
            }
            Close();
        }

        private async void SurfScapeGateway_Loaded(object sender, RoutedEventArgs e)
        {
            // Source - https://stackoverflow.com/a
            // Posted by Joe White, modified by community. See post 'Timeline' for change history
            // Retrieved 2025-11-12, License - CC BY-SA 4.0

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            await _viewModel.InitializeAsync();
        }

        public partial class SurfScapeGatewayViewModel : ObservableObject
        {
            public required Action IsDone { get; set; }
            [ObservableProperty]
            private string currentTask = string.Empty;

            private bool hasOfflineDb = false;

            private readonly bool windowTriggered = false;

            public SurfScapeGatewayViewModel(bool windowTrigger)
            {
                windowTriggered = windowTrigger;
                if (windowTriggered)
                    CurrentTask = "Starting Celer...";
            }
            public async Task InitializeAsync()
            {
                try
                {
                    if (MainConfiguration.Default.EnableAutoSurfScapeGateway || !windowTriggered)
                    {
                        await Task.Delay(200);
                        await SurfScapeWebServices();
                    }
                    CurrentTask = "Starting hardware services...";
                    await SetDxdiag();
                    GenerateBatteryReport();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                IsDone?.Invoke();
            }

            public async Task SurfScapeWebServices()
            {
                CurrentTask = "Checking for an internet connection...";
                bool isOnline = UserLand.IsInternetAvailable();
                if (isOnline)
                {
                    CurrentTask = "Downloading cleaning signatures...";
                    bool success = await CleaningSignatureManager.TryDownloadCleaningSignaturesAsync();
                    if (success)
                    {
                        AppGlobals.EnableCleanEngine = true;
                        CurrentTask = "Signatures updated!";
                    }
                    else
                    {
                        CurrentTask = "Service down. Trying to get local signatures.";
                        SetOfflineDatabase();
                    }
                }
                else
                {
                    CurrentTask = "No internet. Trying to get local signatures.";
                    SetOfflineDatabase();
                    CurrentTask = hasOfflineDb
                        ? "Found local signatures!"
                        : "No local signatures found, cleaning has been disabled";
                }
            }

            public void SetOfflineDatabase()
            {
                hasOfflineDb = CleaningSignatureManager.HasLocalDatabase();
                AppGlobals.EnableCleanEngine = hasOfflineDb;
            }

            private async Task SetDxdiag()
            {
                await Task.Run(async () =>
                {
                    string dxdiagPath = "dxdiag.xml";
                    if (!File.Exists(dxdiagPath))
                    {
                        try
                        {
                            using var proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "dxdiag.exe",
                                    Arguments = "/x dxdiag.xml",
                                    UseShellExecute = true,
                                    CreateNoWindow = true,
                                },
                            };
                            proc.Start();
                            while (!File.Exists(dxdiagPath))
                            {
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            CurrentTask = "Error when running dxdiag! " + ex.Message;
                            Debug.WriteLine("dxdiag failed: " + ex.Message);
                        }
                    }
                });
            }

            private void GenerateBatteryReport()
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/BATTERYREPORT /OUTPUT \"batteryreport.xml\" /XML",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                CurrentTask = "Getting battery information...";
                Process.Start(psi)?.WaitForExit();
            }
        }
    }
}
