using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.ViewModels
{
    public partial class BaseModuleViewModel : ObservableObject
    {
        /// <summary>
        /// Property to define the max width of a module (either auto to fill the window or limit to the default 1100px)
        /// </summary>
        [ObservableProperty]
        private string apperanceUserViewport = MainConfiguration.Default.AppearanceMaxWidth ? "960" : "auto";
    }
}
