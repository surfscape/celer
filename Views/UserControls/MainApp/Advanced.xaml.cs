using Celer.ViewModels;
using System.Windows.Controls;


namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Advanced.xaml
    /// </summary>
    public partial class Advanced : UserControl
    {
        public Advanced()
        {
            InitializeComponent();
            DataContext = new AdvancedViewModel();
        }
    }
}
