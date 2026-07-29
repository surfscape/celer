using Celer.Models.Preferences;
using Celer.Properties;
using Celer.Services;
using Celer.Utilities;
using Celer.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;
using static Celer.Views.Pages.Settings.SettingsAdvancedViewModel;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsInterfaceViewModel : SettingsBaseViewModel
    {
        private readonly SettingsNavigation _settingsNavigation;

        [ObservableProperty]
        public partial Dictionary<string,string> Languages { get; set; } = Localization.languages;

        [ObservableProperty]
        public partial string CurrentLanguage { get; set; } = Localization.GetLanguageValueFromPreference();

        private bool _isRevertingLanguage = false;

        partial void OnCurrentLanguageChanged(string value)
        {
            if (_isRevertingLanguage)
                return;
            System.Windows.MessageBoxResult result = Dialog.Show("To change the language you have to restart the app. Would you like to restart now?", "Celer Configuration Manager", ["Restart"]);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Localization.SetApplicationLanguage(CurrentLanguage);
                WeakReferenceMessenger.Default.Send(new TriggerApplicationClosureMessage(true));
            }
            else if (result == System.Windows.MessageBoxResult.No)
                Localization.SetApplicationLanguage(CurrentLanguage);
            else
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _isRevertingLanguage = true;
                    CurrentLanguage = Localization.GetLanguageValueFromPreference();
                    _isRevertingLanguage = false;
                }));
            }
        }

        [ObservableProperty]
        public partial bool EnableFillContent { get; set; } = MainConfiguration.Default.ViewFillContent;

        partial void OnEnableFillContentChanged(bool value)
        {
            MainConfiguration.Default.ViewFillContent = value;
            MainConfiguration.Default.Save();
            WeakReferenceMessenger.Default.Send(new ViewportChangedMessage(MainConfiguration.Default.ViewFillContent));
        }

        [ObservableProperty]
        public partial bool EnableRounding { get; set; } = MainConfiguration.Default.EnableRounding;

        partial void OnEnableRoundingChanged(bool value)
        {
            MainConfiguration.Default.EnableRounding = value;
            MainConfiguration.Default.Save();
        }

        [ObservableProperty]
        public partial bool SaveSidebarCompactMode { get; set; } = MainConfiguration.Default.SaveSidebarCompactMode;

        partial void OnSaveSidebarCompactModeChanged(bool value)
        {
            MainConfiguration.Default.SaveSidebarCompactMode = value;
            MainConfiguration.Default.Save();
        }
        public ObservableCollection<string> Themes { get; } = ["System", "Light", "Dark"];

        [ObservableProperty]
        public partial string CurrentTheme { get; set; } = MainConfiguration.Default.Theme == (int)CelerTheme.Auto ? "System" : MainConfiguration.Default.Theme == (int)CelerTheme.Light ? "Light" : "Dark";

        partial void OnCurrentThemeChanged(string value)
        {
            MainConfiguration.Default.Theme = CurrentTheme == "System" ? (int)CelerTheme.Auto : CurrentTheme == "Light" ? (int)CelerTheme.Light : (int)CelerTheme.Dark;
            MainConfiguration.Default.Save();
        }

        public SettingsInterfaceViewModel(SettingsNavigation settingsNavigation)
        {
            _settingsNavigation = settingsNavigation;
            _settingsNavigation.PageTitle = "User interface & presentation";
        }

    }
    public class ViewportChangedMessage(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
