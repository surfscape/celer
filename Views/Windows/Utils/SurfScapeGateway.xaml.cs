using Celer.Properties;
using Celer.Resources;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static Celer.Views.Pages.Settings.SettingsModuleCleaningViewModel;

namespace Celer.Views.Windows.Utils
{
	/// <summary>
	/// Interaction logic for SurfScapeGateway, responsible for initializing hardware services and downloading cleaning signatures.
	/// </summary>
	public partial class SurfScapeGateway : Window
	{

		// Source - https://stackoverflow.com/a
		// Posted by Joe White, modified by community.
		// Retrieved 2025-11-12, License - CC BY-SA 4.0
		// Hides the titlebar close button

		private const int GWL_STYLE = -16;
		private const int WS_SYSMENU = 0x80000;
		[LibraryImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
		private static partial int GetWindowLong(IntPtr hWnd, int nIndex);
		[LibraryImport("user32.dll", EntryPoint = "SetWindowLongA")]
		private static partial int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		private readonly SurfScapeGatewayViewModel _viewModel;
		private readonly MainWindow _mainWindow;

		/// <summary>
		/// Used to determine whether the window was triggered on startup or not. This is to make sure that if the user has disabled auto updates, they can still trigger them when gateway is ran manually.
		/// </summary>
		public bool MainWindowTrigger { get; set; } = false;
		/// <summary>
		/// Hides the main window when true. Used when gateway is ran during startup and when Celer is launched with the '--silent' flag.
		/// </summary>
		public bool SilentStartup { get; set; } = false;

		public SurfScapeGateway(MainWindow mainWindow)
		{
			InitializeComponent();
			_mainWindow = mainWindow;
			_viewModel = new SurfScapeGatewayViewModel(MainWindowTrigger) { IsDone = InitializeApp };
			DataContext = _viewModel;
			Loaded += SurfScapeGateway_Loaded;
		}

		private void InitializeApp()
		{
			if (MainWindowTrigger)
			{
				if (SilentStartup)
				{
					_mainWindow.Visibility = Visibility.Collapsed;
					ProcessPowerManager.Enable();
				}
				else
					_mainWindow.Show();
				Close();
			}
			Close();
		}

		private async void SurfScapeGateway_Loaded(object sender, RoutedEventArgs e)
		{
			// Source - https://stackoverflow.com/a
			// Posted by Joe White, modified by community. See post 'Timeline' for change history
			// Retrieved 2025-11-12, License - CC BY-SA 4.0
			var hwnd = new WindowInteropHelper(this).Handle;
			int v = SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
			Debug.WriteLine(v);
			await _viewModel.InitializeAsync();
		}

		public partial class SurfScapeGatewayViewModel : ObservableObject
		{
			public required Action IsDone { get; set; }
			[ObservableProperty]
			public partial string CurrentTask { get; set; } = string.Empty;

			private bool hasOfflineDb = false;

			private readonly bool windowTriggered = false;

			public SurfScapeGatewayViewModel(bool windowTrigger)
			{
				windowTriggered = windowTrigger;
				if (windowTriggered)
					CurrentTask = Resource.GatewayTask_StartCeler;
			}
			public async Task InitializeAsync()
			{
				try
				{
					if (!MainConfiguration.Default.EnableSurfScapeGateway || MainConfiguration.Default.EnableAutoSurfScapeGateway || !windowTriggered)
					{
						await Task.Delay(200);
						await SurfScapeWebServices();
					}
					CurrentTask = Resource.GatewayTask_StartHardwareServices;
					await SetDxdiag();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}
				IsDone?.Invoke();
			}

			public async Task SurfScapeWebServices()
			{
				CurrentTask = Resource.GatewayTask_CheckInternetConnection;
				bool isOnline = UserLand.IsInternetAvailable();
				if (isOnline && MainConfiguration.Default.EnableSurfScapeGateway)
				{
					CurrentTask = Resource.GatewayTask_DownloadCleaningSignatures;
					bool success = await CleaningSignatureManager.TryDownloadCleaningSignaturesAsync();
					if (success)
					{
						WeakReferenceMessenger.Default.Send(new TriggerCleaningSignaturesUpdate(true));
						CurrentTask = Resource.GatewayTask_CleaningSignaturesUpdated;
					}
					else
					{
						CurrentTask = Resource.GatewayTask_CheckLocalSignatures;
						SetOfflineDatabase();
						CurrentTask = hasOfflineDb
							? Resource.GatewayTask_FoundLocalSignatures
							: Resource.GatewayTask_DisableCleaning;
					}
				}
				else
				{
					CurrentTask = Resource.GatewayTask_FoundLocalSignatures;
					SetOfflineDatabase();
					CurrentTask = hasOfflineDb
						? Resource.GatewayTask_FoundLocalSignatures
						: Resource.GatewayTask_DisableCleaning;
				}
			}

			public void SetOfflineDatabase()
			{
				hasOfflineDb = CleaningSignatureManager.HasLocalDatabase() || CleaningSignatureManager.HasInternalDatabase();
				WeakReferenceMessenger.Default.Send(new TriggerCleaningSignaturesUpdate(hasOfflineDb));
			}

			private async Task SetDxdiag()
			{
				await Task.Run(async () =>
				{
					string dxdiagPath = "dxdiag.xml";
					try
					{
						using var proc = new Process
						{
							StartInfo = new ProcessStartInfo
							{
								FileName = "dxdiag.exe",
								Arguments = "/x dxdiag.xml",
								UseShellExecute = true,
								CreateNoWindow = true,
							},
						};
						proc.Start();
						while (!File.Exists(dxdiagPath))
						{
							await Task.Delay(500);
						}
					}
					catch (Exception ex)
					{
						CurrentTask = $"{Resource.GatewayTask_FailDxDiag}\n ${ex.Message}";
						Debug.WriteLine("dxdiag failed: " + ex.Message);
					}
				});
			}
		}
	}
}
