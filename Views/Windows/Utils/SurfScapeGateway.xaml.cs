using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;

namespace Celer.Views.Windows.Utils
{
    /// <summary>
    /// Interaction logic for SurfScapeGateway.xaml
    /// </summary>
    public partial class SurfScapeGateway : Window
    {
        public SurfScapeGateway()
        {
            InitializeComponent();
            var viewModel = new SurfScapeGatewayViewModel
            {
                IsDone = Close
            };
            DataContext = viewModel;
        }

        public partial class SurfScapeGatewayViewModel : ObservableObject
        {
            [ObservableProperty]
            private string? currentTask;

            public Action? IsDone { get; set; }

            public SurfScapeGatewayViewModel()
            {
                // Start async task without blocking constructor
                _ = InitializeAsync();
            }

            private async Task InitializeAsync()
            {
                try
                {
                    CurrentTask = "A procurar por atualizações";
                    await Task.Delay(2000); // Simulate work

                    CurrentTask = "A fazer download de assinaturas de limpeza";
                    await Task.Delay(1500); // Simulate more work

                    IsDone?.Invoke(); // Signal to close
                }
                catch (Exception ex)
                {
                    CurrentTask = $"Ocorreu um erro: {ex.Message}";
                }
            }
        }
    }

}
