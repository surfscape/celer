namespace Celer.Services
{
    public interface ITabLifecycle
    {
        void OnActivated();
        void OnDeactivated();
    }
}
