using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Celer.Views.Pages.Settings
{
    public partial class SettingsBaseViewModel() : ObservableObject, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected new void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Dispose() { }
    }
}
