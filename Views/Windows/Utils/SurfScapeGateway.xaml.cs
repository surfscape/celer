using Celer.Models;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

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

            [ObservableProperty] private string currentTask;

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
                    bool success = await CleaningSignatureManager.TryDownloadCleaningSignaturesAsync();
                    if (success)
                    {
                        AppGlobals.EnableCleanEngine = true;
                        CurrentTask = "Assinaturas atualizadas. A inicar Celer";
                        IsDone?.Invoke();
                    }
                }
                CurrentTask = "Sem internet, a buscar assinaturas locais";
                bool hasLocalDb = CleaningSignatureManager.HasLocalDatabase();
                AppGlobals.EnableCleanEngine = hasLocalDb;
                CurrentTask = hasLocalDb
                    ? "Assinatuas locais encontradas!"
                    : "Clean Engine desligado: assinaturas não encontradas";
                IsDone?.Invoke();
            }
        }
    }
}
