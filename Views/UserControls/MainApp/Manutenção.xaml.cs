using Celer.ViewModels;
using System.Windows.Controls;


namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Manutenção.xaml
    /// </summary>
    public partial class Manutenção : UserControl
    {
        public Manutenção()
        {
            InitializeComponent();
            DataContext = new ManutencaoViewModel();
        }
    }
}
