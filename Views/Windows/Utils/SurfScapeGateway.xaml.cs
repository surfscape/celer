using System.Diagnostics;
using System.IO;
using System.Windows;
using Celer.Models;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Views.Windows.Utils
{
    /// <summary>
    /// Interaction logic for SurfScapeGateway.xaml
    /// </summary>
    public partial class SurfScapeGateway : Window
    {
        private readonly SurfScapeGatewayViewModel _viewModel;

        public SurfScapeGateway()
        {
            InitializeComponent();
            _viewModel = new SurfScapeGatewayViewModel { IsDone = Close };
            DataContext = _viewModel;
            Loaded += SurfScapeGateway_Loaded;
        }

        private async void SurfScapeGateway_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.InitializeAsync();
        }

        public partial class SurfScapeGatewayViewModel : ObservableObject
        {
            public Action? IsDone { get; set; }

            [ObservableProperty]
            private string currentTask;

            private bool hasOfflineDb = false;

            public SurfScapeGatewayViewModel()
            {
                CurrentTask = "A iniciar Celer...";
            }

            public async Task InitializeAsync()
            {
                CurrentTask = "A verificar ligação com a internet...";
                bool isOnline = UserLand.IsInternetAvailable();

                await Task.Delay(500);
                if (isOnline)
                {
                    CurrentTask = "A buscar assinaturas de limpeza...";
                    bool success =
                        await CleaningSignatureManager.TryDownloadCleaningSignaturesAsync();
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
                    CurrentTask = "A inicializar serviços de hardware...";
                    await SetDxdiag();
                }
                IsDone?.Invoke();
            }

            public void SetOfflineDatabase()
            {
                hasOfflineDb = CleaningSignatureManager.HasLocalDatabase();
                AppGlobals.EnableCleanEngine = hasOfflineDb;
            }

            private async Task SetDxdiag()
            {
                await Task.Run(() =>
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
                            do
                            {
                                continue;
                            } while (!File.Exists(dxdiagPath) || !proc.HasExited);
                        }
                        catch (Exception ex)
                        {
                            CurrentTask = "Erro ao iniciar dxdiag: " + ex.Message;
                            Debug.WriteLine("dxdiag failed: " + ex.Message);
                        }
                    }
                });
            }
        }
    }
}
