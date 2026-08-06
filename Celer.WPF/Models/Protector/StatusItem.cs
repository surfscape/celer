using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models.Protector
{
    public partial class StatusItem : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private bool isEnabled;

        public StatusItem(string title, string description, bool isEnabled)
        {
            Title = title;
            Description = description;
            IsEnabled = isEnabled;
        }
    }
}
