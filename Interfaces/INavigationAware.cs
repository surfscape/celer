namespace Celer.Interfaces
{
    /// <summary>
    /// Interface used to clean up and clear resources when a subview is closed
    /// </summary>
    public interface INavigationAware
    {
        void OnNavigatedTo();
        void OnNavigatedFrom();
    }
}
