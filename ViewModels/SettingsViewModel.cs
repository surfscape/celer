using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Celer.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        public Func<
            string,
            string,
            MessageBoxButton,
            MessageBoxImage,
            Task<MessageBoxResult>
        >? ShowDialogAsync { get; set; }
        public Action? CloseWindowAction { get; set; }

        private bool _initialEnableAlertCPUTracking;
        private bool _initialEnableAlertMemoryTracking;
        private bool _initialEnableAlertTrackProcess;
        private string _initialTrackProcess = string.Empty;
        private bool _initialEnableRounding;
        private string _initialCurrentTheme = string.Empty;
        private List<string> _initialPaths = [];
        private bool _initialEnableExportCleaningLog;

        private bool _isUpdatingChildrenFromMaster = false;

        [ObservableProperty]
        private bool enableAlertCPUTracking = MainConfiguration.Default.ALERTS_CPUTrackingEnable;

        partial void OnEnableAlertCPUTrackingChanged(bool value)
        {
            CheckForUnsavedChanges();
            if (!_isUpdatingChildrenFromMaster)
            {
                //Debug.WriteLine($"CHILD: CPU Changed to {value}. Notifying master.");
                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));
            }
        }

        [ObservableProperty]
        private bool enableAlertMemoryTracking = MainConfiguration
            .Default
            .ALERTS_MemoryTrackingEnable;

        partial void OnEnableAlertMemoryTrackingChanged(bool value)
        {
            CheckForUnsavedChanges();
            if (!_isUpdatingChildrenFromMaster)
            {
                //Debug.WriteLine($"CHILD: Memory Changed to {value}. Notifying master.");
                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsProcessTrackingTextBoxEnabled))]
        private bool enableAlertTrackProcess = MainConfiguration.Default.ALERTS_EnableTrackProcess;

        partial void OnEnableAlertTrackProcessChanged(bool value)
        {
            CheckForUnsavedChanges();
            if (!_isUpdatingChildrenFromMaster)
            {
                //Debug.WriteLine($"CHILD: Process Changed to {value}. Notifying master.");
                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                // IsProcessTrackingTextBoxEnabled is also notified by its attribute
            }
        }

        [ObservableProperty]
        private string trackProcess = MainConfiguration.Default.ALERTS_TrackProcess ?? string.Empty;

        partial void OnTrackProcessChanged(string value) => CheckForUnsavedChanges();

        public bool AreInnerAlertsEnabled => EnableAlerts != false;

        public bool IsProcessTrackingTextBoxEnabled =>
            AreInnerAlertsEnabled && EnableAlertTrackProcess;

        [ObservableProperty]
        private bool enableExportCleaningLog = MainConfiguration.Default.CLEANENGINE_ExportLog;

        partial void OnEnableExportCleaningLogChanged(bool value) => CheckForUnsavedChanges();

        public bool? EnableAlerts
        {
            get
            {
                bool allTrue =
                    this.EnableAlertCPUTracking
                    && this.EnableAlertMemoryTracking
                    && this.EnableAlertTrackProcess;
                if (allTrue)
                {
                    //Debug.WriteLine("MASTER GET: All true -> true");
                    return true;
                }

                bool allFalse =
                    !this.EnableAlertCPUTracking
                    && !this.EnableAlertMemoryTracking
                    && !this.EnableAlertTrackProcess;
                if (allFalse)
                {
                    //Debug.WriteLine("MASTER GET: All false -> false");
                    return false;
                }
                //Debug.WriteLine("MASTER GET: Mixed -> null");
                return null;
            }
            set
            {
                //Debug.WriteLine($"MASTER SET: Received value = {value?.ToString() ?? "null"}");
                bool? currentState = GetCurrentAggregatedAlertsStateForSetter();
                //Debug.WriteLine($"MASTER SET: Current aggregated state = {currentState?.ToString() ?? "null"}");

                if (value == currentState)
                {
                    //Debug.WriteLine("MASTER SET: New value is same as current state. No action.");
                    return;
                }

                // This is the key: when the master checkbox is clicked, 'value' should be the target state (true or false).
                // If 'value' is null, it means the checkbox is trying to go to indeterminate,
                // which usually happens if a child was changed.
                // We only want to cascade from master if 'value' is explicitly true or false.
                if (value.HasValue)
                {
                    bool targetChildrenState = value.Value;
                    //Debug.WriteLine($"MASTER SET: value.HasValue is true. Target for children: {targetChildrenState}");

                    _isUpdatingChildrenFromMaster = true;
                    try
                    {
                        //Debug.WriteLine($"MASTER SET: Updating children. Current CPU: {EnableAlertCPUTracking}, Memory: {EnableAlertMemoryTracking}, Process: {EnableAlertTrackProcess}");
                        if (this.EnableAlertCPUTracking != targetChildrenState)
                        {
                            //Debug.WriteLine($"MASTER SET: Setting EnableAlertCPUTracking to {targetChildrenState}");
                            this.EnableAlertCPUTracking = targetChildrenState;
                        }
                        if (this.EnableAlertMemoryTracking != targetChildrenState)
                        {
                            //Debug.WriteLine($"MASTER SET: Setting EnableAlertMemoryTracking to {targetChildrenState}");
                            this.EnableAlertMemoryTracking = targetChildrenState;
                        }
                        if (this.EnableAlertTrackProcess != targetChildrenState)
                        {
                            //Debug.WriteLine($"MASTER SET: Setting EnableAlertTrackProcess to {targetChildrenState}");
                            this.EnableAlertTrackProcess = targetChildrenState;
                        }
                    }
                    finally
                    {
                        _isUpdatingChildrenFromMaster = false;
                        //Debug.WriteLine("MASTER SET: Finished updating children.");
                    }
                }
                else
                {
                    //Debug.WriteLine("MASTER SET: value is null. No direct update to children from master.");
                    // If value is null, it means the master is becoming indeterminate.
                    // This typically happens because a child was changed, not because the user
                    // clicked the master to specifically make it indeterminate.
                    // So, we don't propagate 'null' to children.
                }

                // Crucially, notify after all potential changes.
                //Debug.WriteLine("MASTER SET: Notifying property changes for EnableAlerts and dependents.");
                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));
            }
        }

        private bool? GetCurrentAggregatedAlertsStateForSetter()
        {
            bool allTrue =
                this.EnableAlertCPUTracking
                && this.EnableAlertMemoryTracking
                && this.EnableAlertTrackProcess;
            if (allTrue)
                return true;
            bool allFalse =
                !this.EnableAlertCPUTracking
                && !this.EnableAlertMemoryTracking
                && !this.EnableAlertTrackProcess;
            if (allFalse)
                return false;
            return null;
        }

        // ... (Rest of your ViewModel as before)
        [ObservableProperty]
        private bool enableRounding = MainConfiguration.Default.EnableRounding;

        partial void OnEnableRoundingChanged(bool value) => CheckForUnsavedChanges();

        public ObservableCollection<string> ComboOptions { get; } = ["Steel WPF Dark"];

        [ObservableProperty]
        private string currentTheme = MainConfiguration.Default.Theme ?? "Steel WPF Dark";

        partial void OnCurrentThemeChanged(string value) => CheckForUnsavedChanges();

        public ObservableCollection<string> Paths { get; } =
            new ObservableCollection<string>(
                MainConfiguration.Default.CLEANENGINE_CustomPaths?.Cast<string>()
                    ?? Enumerable.Empty<string>()
            );

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
            _initialPaths = new List<string>(Paths);
            _initialEnableExportCleaningLog = EnableExportCleaningLog;
        }

        private void OnPathsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CheckForUnsavedChanges();
        }

        private void CheckForUnsavedChanges()
        {
            bool changed =
                EnableAlertCPUTracking != _initialEnableAlertCPUTracking
                || EnableAlertMemoryTracking != _initialEnableAlertMemoryTracking
                || EnableAlertTrackProcess != _initialEnableAlertTrackProcess
                || !string.Equals(
                    TrackProcess,
                    _initialTrackProcess,
                    System.StringComparison.Ordinal
                )
                || EnableRounding != _initialEnableRounding
                || !string.Equals(
                    CurrentTheme,
                    _initialCurrentTheme,
                    System.StringComparison.Ordinal
                )
                || EnableExportCleaningLog != _initialEnableExportCleaningLog;

            if (!changed)
            {
                if (!Paths.SequenceEqual(_initialPaths))
                {
                    changed = true;
                }
            }
            HasUnsavedChanges = changed;
        }

        private void RevertChanges()
        {
            _isUpdatingChildrenFromMaster = true; // Prevent notifications during revert
            try
            {
                Paths.CollectionChanged -= OnPathsCollectionChanged;

                EnableAlertCPUTracking = _initialEnableAlertCPUTracking;
                EnableAlertMemoryTracking = _initialEnableAlertMemoryTracking;
                EnableAlertTrackProcess = _initialEnableAlertTrackProcess;
                TrackProcess = _initialTrackProcess;
                EnableRounding = _initialEnableRounding;
                CurrentTheme = _initialCurrentTheme;
                EnableExportCleaningLog = _initialEnableExportCleaningLog;

                Paths.Clear();
                foreach (var p in _initialPaths)
                {
                    Paths.Add(p);
                }
                Paths.CollectionChanged += OnPathsCollectionChanged;
            }
            finally
            {
                _isUpdatingChildrenFromMaster = false;
            }
            // After reverting, notify all relevant properties to update UI
            OnPropertyChanged(nameof(EnableAlertCPUTracking));
            OnPropertyChanged(nameof(EnableAlertMemoryTracking));
            OnPropertyChanged(nameof(EnableAlertTrackProcess));
            OnPropertyChanged(nameof(TrackProcess));
            OnPropertyChanged(nameof(EnableRounding));
            OnPropertyChanged(nameof(CurrentTheme));
            OnPropertyChanged(nameof(EnableExportCleaningLog));
            OnPropertyChanged(nameof(Paths)); // May not be necessary if ListBox updates from ObservableCollection events

            OnPropertyChanged(nameof(EnableAlerts));
            OnPropertyChanged(nameof(AreInnerAlertsEnabled));
            OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));

            CheckForUnsavedChanges(); // Should be false now
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
            MainConfiguration.Default.CLEANENGINE_ExportLog = EnableExportCleaningLog;

            var sc = new System.Collections.Specialized.StringCollection();
            foreach (var p in Paths)
                sc.Add(p);
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
            if (HasUnsavedChanges)
            {
                Save();
            }
            CloseWindowAction?.Invoke();
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            if (HasUnsavedChanges)
            {
                if (ShowDialogAsync == null)
                {
                    CloseWindowAction?.Invoke();
                    return;
                }

                var result = await ShowDialogAsync(
                    "Unsaved Changes",
                    "You have unsaved changes. Would you like to save them?",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning
                );

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
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    Save();
                    return true;
                }
                else if (result == MessageBoxResult.No)
                {
                    RevertChanges();
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
