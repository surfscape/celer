using System.Windows.Controls;

namespace Celer.Models
{
    public class NavigationSubView(string name, string description, UserControl control)
    {
        public string Name { get; } = name;
        public string Description { get; set; } = description;
        public UserControl Control { get; } = control;
    }
}
