using Celer.ViewModels.OptimizationVM;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp.OptimizationViews
{
    /// <summary>
    /// Interaction logic for MemoryManagement.xaml
    /// </summary>
    public partial class MemoryManagement : UserControl
    {
        public MemoryManagement(MemoryViewModel memoryViewModel)
        {
            InitializeComponent();
            DataContext = memoryViewModel;
        }
    }
}
