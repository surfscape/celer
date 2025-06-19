using System.Collections.ObjectModel;
using System.Diagnostics;
using Celer.Models.Protector;
using Celer.Services.OpsecEngine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Celer.ViewModels.OpsecVM
{
    public partial class OverviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private int privacyScore;

        [ObservableProperty]
        private int securityScore;

        public ObservableCollection<StatusItem> PrivacyItems { get; } = [];
        public ObservableCollection<StatusItem> SecurityItems { get; } = [];

        public OverviewViewModel() { }

        [RelayCommand]
        public async Task RefreshAsync()
        {
            PrivacyItems.Clear();
            SecurityItems.Clear();

            var (privacy, privacyScore) = await PrivacyEvaluator.EvaluateAsync();
            foreach (var item in privacy)
                PrivacyItems.Add(item);
            PrivacyScore = privacyScore;

            var (security, securityScore) = await SecurityEvaluator.EvaluateAsync();
            foreach (var item in security)
                SecurityItems.Add(item);
            SecurityScore = securityScore;
        }

        [RelayCommand]
        public void OpenDefender()
        {
            Process.Start(
                new ProcessStartInfo("ms-settings:windowsdefender") { UseShellExecute = true }
            );
        }

        [RelayCommand]
        public void OpenPrivacySettings()
        {
            Process.Start(new ProcessStartInfo("ms-settings:privacy") { UseShellExecute = true });
        }
    }
}
