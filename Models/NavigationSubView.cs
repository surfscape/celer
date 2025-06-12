using System.Windows.Controls;

namespace Celer.Models
{
    public class NavigationSubView(string name, UserControl control)
    {
        public string Name { get; } = name;
        public UserControl Control { get; } = control;
    }
}
