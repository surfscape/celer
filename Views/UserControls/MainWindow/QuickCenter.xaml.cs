using ByteSizeLib;
using Celer.Properties;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;


namespace Celer.Views.UserControls.MainWindow
{
    /// <summary>
    /// Interaction logic for QuickCenter.xaml
    /// </summary>
    public partial class QuickCenter : UserControl
    {
        private readonly QuickCenterViewModel viewModel;
        public QuickCenter()
        {
            InitializeComponent();
            viewModel = new QuickCenterViewModel();
            DataContext = viewModel;
            IsVisibleChanged += new DependencyPropertyChangedEventHandler(
            CheckVisibility);
        }

        void CheckVisibility(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                viewModel.Initialize();
            }
        }
    }

    public partial class QuickCenterViewModel : ObservableObject
    {
        /* used to check if the recycle bin is empty or not*/
        [ObservableProperty]
        private bool canRecycleBin = false;

        /* used to check if the recycle bin is currently being cleaned */
        [ObservableProperty]
        private bool isRecycleBin = false;

        [ObservableProperty]
        private double recycleTotalSize = 0;

        [ObservableProperty]
        private double recycleCleaned = 0;

        [ObservableProperty]
        private bool isCleaning = false;

        [ObservableProperty]
        private double tempFiles = 0;

        [ObservableProperty]
        private double tempFilesCleaned = 0;

        private readonly string recycleBinPath = "C:\\$Recycle.Bin\\" + WindowsIdentity.GetCurrent().User + "\\";
        public QuickCenterViewModel()
        {
            Initialize();
        }

        public void Initialize()
        {
            IsRecycleBin = false;
            IsCleaning = false;
            if (Directory.Exists(recycleBinPath) && Directory.GetFiles(recycleBinPath).Length >= 2)
                CanRecycleBin = true;
        }

        [RelayCommand]
        private void EmptyRecycleBin()
        {
            CanRecycleBin = false;
            IsRecycleBin = true;
            long size = 0;
            DirectoryInfo dir = new DirectoryInfo(recycleBinPath);
            foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
            }
            var byteTotalSize = ByteSize.FromBytes(size);
            RecycleTotalSize = byteTotalSize.MegaBytes;
            size = 0;
            foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
                fi.Delete();
            }
            var byteFileSize = ByteSize.FromBytes(size);
            RecycleCleaned = MainConfiguration.Default.EnableRounding ? Math.Round(byteFileSize.MegaBytes, 1) : byteFileSize.MegaBytes;
        }

        [RelayCommand]
        private void DeleteTempFiles()
        {
            IsCleaning = true;
            long size = 0;
            string windowsTemp = Environment.ExpandEnvironmentVariables("%SystemRoot%\\Temp");
            DirectoryInfo tempDir = new(windowsTemp);
            foreach (FileInfo fi in tempDir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
            }
            string userTemp = Environment.ExpandEnvironmentVariables("%TEMP%");
            DirectoryInfo userTempDir = new(userTemp);
            foreach (FileInfo fi in userTempDir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
            }
            var byteTotalSize = ByteSize.FromBytes(size);
            TempFiles = byteTotalSize.MegaBytes;
            size = 0;
            foreach (FileInfo fi in tempDir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
                var byteSize = ByteSize.FromBytes(size);
                TempFilesCleaned = MainConfiguration.Default.EnableRounding ? Math.Round(byteSize.MegaBytes, 1) : byteSize.MegaBytes;
                try
                {
                    fi.Delete();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

            }
            foreach (FileInfo fi in userTempDir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += fi.Length;
                var byteSize = ByteSize.FromBytes(size);
                TempFilesCleaned = MainConfiguration.Default.EnableRounding ? Math.Round(byteSize.MegaBytes, 1) : byteSize.MegaBytes;
                try
                {
                    fi.Delete();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        [RelayCommand]
        private static void QCExitApp()
        {
            Application.Current.Shutdown();
        }
    }
}
