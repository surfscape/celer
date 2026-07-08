using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsGeneralViewModel : SettingsBaseViewModel
    {
        [ObservableProperty]
        public partial bool EnableFillContent { get; set; } = MainConfiguration.Default.ViewFillContent;


        partial void OnEnableFillContentChanged(bool value)
        {
            MainConfiguration.Default.ViewFillContent = value;
            MainConfiguration.Default.Save();
            WeakReferenceMessenger.Default.Send(new ViewportChangedMessage(MainConfiguration.Default.ViewFillContent));
        }
    }
    public class ViewportChangedMessage(bool value) : ValueChangedMessage<bool>(value)
    {
    }
}
