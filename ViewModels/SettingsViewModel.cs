using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Celer.Properties;
using System.Windows;

namespace Celer.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public Func<string, string, MessageBoxButton, MessageBoxImage, Task<MessageBoxResult>>? ShowDialogAsync { get; set; }
        public Action? CloseWindowAction { get; set; }

        private bool _initialEnableAlertCPUTracking;
        private bool _initialEnableAlertMemoryTracking;
        private bool _initialEnableAlertTrackProcess;
        private string _initialTrackProcess = string.Empty;
        private bool _initialEnableRounding;
        private string _initialCurrentTheme = string.Empty;
        private List<string> _initialPaths = [];
        private bool _initialEnableExportCleaningLog;

        /// <summary>
        /// Varibles that will be binded to the view
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAlerts))]
        private bool enableAlertCPUTracking = MainConfiguration.Default.ALERTS_CPUTrackingEnable;
        partial void OnEnableAlertCPUTrackingChanged(bool value) => CheckForUnsavedChanges();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAlerts))]
        private bool enableAlertMemoryTracking = MainConfiguration.Default.ALERTS_MemoryTrackingEnable;
        partial void OnEnableAlertMemoryTrackingChanged(bool value) => CheckForUnsavedChanges();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EnableAlerts))]
        [NotifyPropertyChangedFor(nameof(IsProcessTrackingTextBoxEnabled))]
        private bool enableAlertTrackProcess = MainConfiguration.Default.ALERTS_EnableTrackProcess;
        partial void OnEnableAlertTrackProcessChanged(bool value) => CheckForUnsavedChanges();

        [ObservableProperty]
        private string trackProcess = MainConfiguration.Default.ALERTS_TrackProcess ?? string.Empty;
        partial void OnTrackProcessChanged(string value) => CheckForUnsavedChanges();
        public bool AreInnerAlertsEnabled => EnableAlerts != false;
        public bool IsProcessTrackingTextBoxEnabled => AreInnerAlertsEnabled && EnableAlertTrackProcess;

        /// <summary>
        /// Clean Engine preferences
        /// </summary>
        [ObservableProperty]
        private bool enableExportCleaningLog = MainConfiguration.Default.CLEANENGINE_ExportLog;
        partial void OnEnableExportCleaningLogChanged(bool value) => CheckForUnsavedChanges();

        public bool? EnableAlerts
        {
            get
            {
                bool allTrue = EnableAlertCPUTracking && EnableAlertMemoryTracking && EnableAlertTrackProcess;
                if (allTrue) return true;

                bool allFalse = !EnableAlertCPUTracking && !EnableAlertMemoryTracking && !EnableAlertTrackProcess;
                if (allFalse) return false;

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    bool targetChildrenState = value.Value;

                    if (EnableAlertCPUTracking != targetChildrenState)
                        EnableAlertCPUTracking = targetChildrenState;
                    if (EnableAlertMemoryTracking != targetChildrenState)
                        EnableAlertMemoryTracking = targetChildrenState;
                    if (EnableAlertTrackProcess != targetChildrenState)
                        EnableAlertTrackProcess = targetChildrenState;
                }
                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));
            }
        }

        [ObservableProperty]
        private bool enableRounding = MainConfiguration.Default.EnableRounding;
        partial void OnEnableRoundingChanged(bool value) => CheckForUnsavedChanges();

        public ObservableCollection<string> ComboOptions { get; } =
            ["Steel WPF Dark"];

        [ObservableProperty]
        private string currentTheme = MainConfiguration.Default.Theme ?? "Steel WPF Dark";
        partial void OnCurrentThemeChanged(string value) => CheckForUnsavedChanges();

        public ObservableCollection<string> Paths { get; } =
            new ObservableCollection<string>(MainConfiguration.Default.CLEANENGINE_CustomPaths?.Cast<string>() ?? []);

        [ObservableProperty]
        private string newPath = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
        private bool hasUnsavedChanges;

        public SettingsViewModel()
        {
            StoreInitialValues();
            Paths.CollectionChanged += OnPathsCollectionChanged;
        }

        private void StoreInitialValues()
        {
            _initialEnableAlertCPUTracking = EnableAlertCPUTracking;
            _initialEnableAlertMemoryTracking = EnableAlertMemoryTracking;
            _initialEnableAlertTrackProcess = EnableAlertTrackProcess;
            _initialTrackProcess = TrackProcess;
            _initialEnableRounding = EnableRounding;
            _initialCurrentTheme = CurrentTheme;
            _initialPaths = [.. Paths];
            _initialEnableExportCleaningLog = EnableExportCleaningLog;
        }

        private void OnPathsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CheckForUnsavedChanges();
        }

        private void CheckForUnsavedChanges()
        {
            bool changed = EnableAlertCPUTracking != _initialEnableAlertCPUTracking ||
                           EnableAlertMemoryTracking != _initialEnableAlertMemoryTracking ||
                           EnableAlertTrackProcess != _initialEnableAlertTrackProcess ||
                           !string.Equals(TrackProcess, _initialTrackProcess, System.StringComparison.Ordinal) ||
                           EnableRounding != _initialEnableRounding ||
                           !string.Equals(CurrentTheme, _initialCurrentTheme, System.StringComparison.Ordinal) || enableExportCleaningLog != _initialEnableExportCleaningLog;

            if (!changed)
            {
                var pathsSet = new HashSet<string>(Paths);
                var initialPathsSet = new HashSet<string>(_initialPaths);
                if (!pathsSet.SetEquals(initialPathsSet))
                {
                    changed = true;
                }
            }
            HasUnsavedChanges = changed;
        }

        private void RevertChanges()
        {
            Paths.CollectionChanged -= OnPathsCollectionChanged;

            EnableAlertCPUTracking = _initialEnableAlertCPUTracking;
            EnableAlertMemoryTracking = _initialEnableAlertMemoryTracking;
            EnableAlertTrackProcess = _initialEnableAlertTrackProcess;
            TrackProcess = _initialTrackProcess;
            EnableRounding = _initialEnableRounding;
            CurrentTheme = _initialCurrentTheme;

            Paths.Clear();
            foreach (var p in _initialPaths)
            {
                Paths.Add(p);
            }

            Paths.CollectionChanged += OnPathsCollectionChanged;

            CheckForUnsavedChanges();
        }

        [RelayCommand]
        private void Save()
        {
            MainConfiguration.Default.ALERTS_CPUTrackingEnable = EnableAlertCPUTracking;
            MainConfiguration.Default.ALERTS_MemoryTrackingEnable = EnableAlertMemoryTracking;
            MainConfiguration.Default.ALERTS_EnableTrackProcess = EnableAlertTrackProcess;
            MainConfiguration.Default.ALERTS_TrackProcess = TrackProcess;
            MainConfiguration.Default.EnableRounding = EnableRounding;
            MainConfiguration.Default.Theme = CurrentTheme;

            var sc = new StringCollection();
            foreach (var p in Paths) sc.Add(p);
            MainConfiguration.Default.CLEANENGINE_CustomPaths = sc;

            MainConfiguration.Default.Save();

            StoreInitialValues();
            CheckForUnsavedChanges();
        }

        [RelayCommand(CanExecute = nameof(CanApply))]
        private void Apply()
        {
            Save();
        }
        private bool CanApply() => HasUnsavedChanges;


        [RelayCommand] 
        private void Ok()
        {
            Save();
            CloseWindowAction?.Invoke();
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (HasUnsavedChanges)
            {
                if (ShowDialogAsync == null) { CloseWindowAction?.Invoke(); return; }

                var result = await ShowDialogAsync(
                    "Unsaved Changes",
                    "You have unsaved changes. Would you like to save them?",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    CloseWindowAction?.Invoke();
                }
                else if (result == MessageBoxResult.No)
                {
                    RevertChanges();
                    CloseWindowAction?.Invoke();
                }
               
            }
            else
            {
                CloseWindowAction?.Invoke();
            }
        }
        public async Task<bool> HandleWindowCloseRequestAsync()
        {
            if (HasUnsavedChanges)
            {
                if (ShowDialogAsync == null)
                {
                    return false;
                }

                var result = await ShowDialogAsync(
                    "Unsaved Changes",
                    "You have unsaved changes. Would you like to save them before closing?",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    return true;
                }
                else if (result == MessageBoxResult.No)
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
            return true;
        }


        [RelayCommand]
        private void PickAndAddPath()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Escolha uma pasta para adicionar.",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
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