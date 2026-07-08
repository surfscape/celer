using Celer.Properties;
using Celer.Views.Pages.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;

namespace Celer.ViewModels
{
    public partial class BaseModuleViewModel : ObservableObject
    { 
        /// <summary>
        /// Property that tracks the current loading progress of the module's view module
        /// </summary>
        [ObservableProperty]
        public partial bool IsLoading { get; set; } = true;

        /// <summary>
        /// Property to define the max width of a module (either auto to fill the window or limit to the default 1100px)
        /// </summary>
        [ObservableProperty]
        public partial string ApperanceUserViewport { get; set; }


        public BaseModuleViewModel()
        {
            ApperanceUserViewport = MainConfiguration.Default.ViewFillContent ? "auto" : "980";
            WeakReferenceMessenger.Default.Register<ViewportChangedMessage>(this, (r, m) =>
            {
                ApperanceUserViewport = m.Value ? "auto" : "980";
            });
        }
    }
}
