using System.Windows.Controls;

namespace Celer.Models
{
    public class NavigationSubView
    {
        public string Name { get; }
        public UserControl Control { get; }

        public NavigationSubView(string name, UserControl control)
        {
            Name = name;
            Control = control;
        }
    }
}
