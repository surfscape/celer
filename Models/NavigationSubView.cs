using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models
{
    public class NavigationSubView(string name, string description, ObservableObject control)
    {
        public string Name { get; } = name;
        public string Description { get; set; } = description;
        public ObservableObject Control { get; } = control;
    }
}
