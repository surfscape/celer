using Celer.ViewModels;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp
{
    /// <summary>
    /// Interaction logic for Limpeza.xaml
    /// </summary>
    public partial class Limpeza : UserControl
    {
        public Limpeza()
        {
            InitializeComponent();
            DataContext = new CleanEngine();
        }
    }
}
