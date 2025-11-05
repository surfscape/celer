using Celer.Models;
using Celer.Properties;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Celer.Views.Windows.Utils
{
    /// <summary>
    /// Interaction logic for SurfScapeGateway.xaml
    /// </summary>
    public partial class SurfScapeGateway : Window
    {
        private readonly SurfScapeGatewayViewModel _viewModel;
        private readonly MainWindow _mainWindow;

        public bool? MainWindowTrigger { get; set; } = false;

        public SurfScapeGateway(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _viewModel = new SurfScapeGatewayViewModel { IsDone = InitializeApp };
            DataContext = _viewModel;
            Loaded += SurfScapeGateway_Loaded;
        }

        private void InitializeApp()
        {
            if (MainWindowTrigger == true)
            {
                _mainWindow.Show();
            }
            Close();
        }

        private async void SurfScapeGateway_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }

        public partial class SurfScapeGatewayViewModel : ObservableObject
        {
            public required Action IsDone { get; set; }
            [ObservableProperty]
            private string currentTask;

            private bool hasOfflineDb = false;

            public SurfScapeGatewayViewModel()
            {
                CurrentTask = "A iniciar Celer...";
                CurrentTask = "Starting Celer...";
            }
            public async Task InitializeAsync()
            {
                try
                {
                    if (MainConfiguration.Default.EnableAutoSurfScapeGateway)
                    {
                        await Task.Delay(200);
                        await SurfScapeWebServices();
                    }
                    CurrentTask = "A inicializar serviços de hardware...";
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
                CurrentTask = "A verificar ligação com a internet...";
                bool isOnline = UserLand.IsInternetAvailable();
                if (isOnline)
                {
                    CurrentTask = "A buscar assinaturas de limpeza...";
                    bool success = await CleaningSignatureManager.TryDownloadCleaningSignaturesAsync();
                    if (success)
                    {
                        AppGlobals.EnableCleanEngine = true;
                        CurrentTask = "Assinaturas atualizadas. A inicar Celer";
                    }
                    else
                    {
                        CurrentTask = "Servidor offline, a buscar assinaturas locais";
                        SetOfflineDatabase();
                    }
                }
                else
                {
                    CurrentTask = "Sem internet, a buscar assinaturas locais";
                    SetOfflineDatabase();
                    CurrentTask = hasOfflineDb
                        ? "Assinatuas locais encontradas!"
                        : "Clean Engine desligado: assinaturas não encontradas";
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
                            var proc = new Process
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
                                await Task.Delay(200);
                            }
                        }
                        catch (Exception ex)
                        {
                            CurrentTask = "Erro ao iniciar dxdiag: " + ex.Message;
                            Debug.WriteLine("dxdiag failed: " + ex.Message);
                        }
                    }
                });
            }

            private static void GenerateBatteryReport()
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = $"/BATTERYREPORT /OUTPUT \"batteryreport.xml\" /XML",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                Process.Start(psi)?.WaitForExit();
            }
        }
    }
}
