using Celer.Models.Protector;
using Celer.Services.OpsecEngine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Celer.ViewModels.OpsecVM
{
    public partial class OverviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private int privacyTotalScore;

        [ObservableProperty]
        private int securityTotalScore;

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
            PrivacyTotalScore = privacyScore;

            var (security, securityScore) = await SecurityEvaluator.EvaluateAsync();
            foreach (var item in security)
                SecurityItems.Add(item);
            SecurityTotalScore = securityScore;
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
