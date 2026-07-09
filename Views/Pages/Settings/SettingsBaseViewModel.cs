using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsBaseViewModel() : ObservableObject, IDisposable
    {
        public virtual void Dispose() { }
    }
}
