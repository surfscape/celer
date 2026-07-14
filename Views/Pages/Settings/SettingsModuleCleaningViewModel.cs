using Celer.Properties;
using Celer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsModuleCleaningViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;

        [ObservableProperty]
        public partial bool EnableExportCleaningLog { get; set; } = MainConfiguration.Default.CLEANENGINE_ExportLog;


        public void OnEnableExportCleaningLog(bool value)
        {
            MainConfiguration.Default.CLEANENGINE_ExportLog = value;
            MainConfiguration.Default.Save();
        }

        [ObservableProperty]
        public partial ObservableCollection<string> Paths { get; set; } = new ObservableCollection<string>(MainConfiguration.Default.CLEANENGINE_CustomPaths?.Cast<string>() ?? [] );

        public SettingsModuleCleaningViewModel(SettingsNavigation settingsNavigation)
        {
            _settingsNavigation = settingsNavigation;
            _settingsNavigation.PageTitle = "Cleaning options";
            Paths.CollectionChanged += (s, e) =>
            {
                Debug.WriteLine($"Collection changed! Action: {e.Action}");
            };
}
            [RelayCommand]
        private void PickAndAddPath()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Choose a path to add",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
            };

            if (dialog.ShowDialog() == true && !Paths.Contains(dialog.SelectedPath))
            {
                Paths.Add(dialog.SelectedPath);
                
            }
        }

        

        [RelayCommand]
        private void RemovePath(string? pathToRemove)
        {
            if (pathToRemove != null && Paths.Contains(pathToRemove))
            {
                Paths.Remove(pathToRemove);
            }
        }


    }
}
