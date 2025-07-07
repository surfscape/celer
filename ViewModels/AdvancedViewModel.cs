using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Celer.ViewModels
{
    public partial class AdvancedViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool enableSchoolFeatures = MainConfiguration.Default.EnableSchoolFeatures;

        public AdvancedViewModel()
        {
            MainConfiguration.Default.PropertyChanged += Configuration_Changed;
        }

        private void Configuration_Changed(object? sender, PropertyChangedEventArgs e)
        {
            EnableSchoolFeatures = MainConfiguration.Default.EnableSchoolFeatures;
        }
    }
}
