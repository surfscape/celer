using System.Management;
using System.Windows;

namespace Celer.Views.Windows.Dialogs
{
    /// <summary>
    /// Interaction logic for SchoolKeyDialog.xaml
    /// </summary>
    public partial class SchoolKeyDialog : Window
    {
        public string? EnteredText { get; private set; }

        public SchoolKeyDialog()
        {
            InitializeComponent();
            GetModel();
        }

        private void GetModel()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Manufacturer, Model FROM Win32_ComputerSystem"
                );
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string manufacturer = obj["Manufacturer"]?.ToString() ?? "Desconhecido";
                    string model = obj["Model"]?.ToString() ?? "Desconhecido";
                    Builder.Text = manufacturer;
                    Model.Text = model;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao obter informações do sistema: " + ex.Message);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            EnteredText = KeyInput.Password;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
