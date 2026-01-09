using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;

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
        >? ShowDialogAsync
        { get; set; }
        public Action? CloseWindowAction { get; set; }

        private bool _initialEnableAlertCPUTracking;
        private bool _initialEnableAlertMemoryTracking;
        private bool _initialEnableAlertTrackProcess;
        private string _initialTrackProcess = string.Empty;
        private bool _initialEnableRounding;
        private bool _initialSaveSidebarCompactMode;
        private string _initialCurrentTheme = string.Empty;
        private string _initialGraphicRenderingMode = string.Empty;
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
                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
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
                bool allChecked = EnableAlertCPUTracking && EnableAlertMemoryTracking && EnableAlertTrackProcess;
                if (allChecked) return true;

                bool noneChecked = !EnableAlertCPUTracking && !EnableAlertMemoryTracking && !EnableAlertTrackProcess;
                if (noneChecked) return false;

                return null;
            }
            set
            {
                if (EnableAlerts == true && value == false)
                {
                    OnPropertyChanged(nameof(EnableAlerts));
                    return;
                }

                if (EnableAlertCPUTracking && EnableAlertMemoryTracking && EnableAlertTrackProcess)
                {
                    value = false;
                    EnableAlertCPUTracking = false;
                    EnableAlertMemoryTracking = false;
                    EnableAlertTrackProcess = false;
                    OnPropertyChanged(nameof(EnableAlerts));
                    OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                    OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));

                }

                if (value.HasValue)
                {
                    bool targetState = value.Value;
                    _isUpdatingChildrenFromMaster = true;
                    try
                    {
                        EnableAlertCPUTracking = targetState;
                        EnableAlertMemoryTracking = targetState;
                        EnableAlertTrackProcess = targetState;
                    }
                    finally
                    {
                        _isUpdatingChildrenFromMaster = false;
                    }
                }

                OnPropertyChanged(nameof(EnableAlerts));
                OnPropertyChanged(nameof(AreInnerAlertsEnabled));
                OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));
            }
        }

        [ObservableProperty]
        private bool enableRounding = MainConfiguration.Default.EnableRounding;

        partial void OnEnableRoundingChanged(bool value) => CheckForUnsavedChanges();

        [ObservableProperty]
        private bool saveSidebarCompactMode = MainConfiguration.Default.SaveSidebarCompactMode;

        partial void OnSaveSidebarCompactModeChanged(bool value) {
            CheckForUnsavedChanges();
            MainConfiguration.Default.SidebarCompactMode = false;
        }

        public ObservableCollection<string> Themes { get; } = ["System","Light", "Dark"];

        [ObservableProperty]
        private string currentTheme = MainConfiguration.Default.Theme == 0 ? "System" : MainConfiguration.Default.Theme == 1 ? "Light" : "Dark";

        public ObservableCollection<string> RenderingModes { get; } = ["Auto", "Hardware (default)", "Software only"];

        [ObservableProperty]
        public string graphicRenderingMode = MainConfiguration.Default.GraphicRenderingMode == 0 ? "Auto" : MainConfiguration.Default.GraphicRenderingMode == 1 ? "Hardware (default)" : "Software only";

        partial void OnGraphicRenderingModeChanged(string value) => CheckForUnsavedChanges();

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
            _initialSaveSidebarCompactMode = SaveSidebarCompactMode;
            _initialCurrentTheme = CurrentTheme;
            _initialPaths = new List<string>(Paths);
            _initialEnableExportCleaningLog = EnableExportCleaningLog;
            _initialGraphicRenderingMode = GraphicRenderingMode;
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
                || EnableRounding != _initialEnableRounding || SaveSidebarCompactMode != _initialSaveSidebarCompactMode
                || !string.Equals(
                    CurrentTheme,
                    _initialCurrentTheme,
                    System.StringComparison.Ordinal
                )
                || EnableExportCleaningLog != _initialEnableExportCleaningLog
            || !string.Equals(
                    GraphicRenderingMode,
                    _initialGraphicRenderingMode,
                    System.StringComparison.Ordinal
                );

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
            _isUpdatingChildrenFromMaster = true;
            try
            {
                Paths.CollectionChanged -= OnPathsCollectionChanged;

                EnableAlertCPUTracking = _initialEnableAlertCPUTracking;
                EnableAlertMemoryTracking = _initialEnableAlertMemoryTracking;
                EnableAlertTrackProcess = _initialEnableAlertTrackProcess;
                TrackProcess = _initialTrackProcess;
                EnableRounding = _initialEnableRounding;
                SaveSidebarCompactMode = _initialSaveSidebarCompactMode;
                CurrentTheme = _initialCurrentTheme;
                EnableExportCleaningLog = _initialEnableExportCleaningLog;
                GraphicRenderingMode = _initialGraphicRenderingMode;

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
            OnPropertyChanged(nameof(EnableAlertCPUTracking));
            OnPropertyChanged(nameof(EnableAlertMemoryTracking));
            OnPropertyChanged(nameof(EnableAlertTrackProcess));
            OnPropertyChanged(nameof(TrackProcess));
            OnPropertyChanged(nameof(EnableRounding));
            OnPropertyChanged(nameof(SaveSidebarCompactMode));
            OnPropertyChanged(nameof(CurrentTheme));
            OnPropertyChanged(nameof(EnableExportCleaningLog));
            OnPropertyChanged(nameof(Paths));

            OnPropertyChanged(nameof(EnableAlerts));
            OnPropertyChanged(nameof(AreInnerAlertsEnabled));
            OnPropertyChanged(nameof(IsProcessTrackingTextBoxEnabled));

            OnPropertyChanged(nameof(GraphicRenderingMode));

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
            MainConfiguration.Default.SaveSidebarCompactMode = SaveSidebarCompactMode;
            MainConfiguration.Default.Theme = CurrentTheme == "System" ? 0 : CurrentTheme == "Light" ? 1 : 2;
            MainConfiguration.Default.CLEANENGINE_ExportLog = EnableExportCleaningLog;
            MainConfiguration.Default.GraphicRenderingMode = GraphicRenderingMode == "Auto" ? 0 : GraphicRenderingMode == "Hardware (default)" ? 1 : 2;
            var sc = new StringCollection();
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
                    "You have unsaved changes. Would you like to save them before closing?",
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

        [RelayCommand]
        private void ResetAppData()
        {
            if (ShowDialogAsync == null)
            {
                return;
            }
            var result = ShowDialogAsync(
                "Celer System",
                "Would you like to close Celer and reset it's settings and data?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            ).Result;
            if (result == MessageBoxResult.Yes)
            {
                MainConfiguration.Default.Reset();
                MainConfiguration.Default.Save();
                File.Delete("batteryreport.xml");
                File.Delete("signatures.json");
                File.Delete("dxdiag.xml");
                Application.Current.Shutdown();
            }
        }
    }
}
