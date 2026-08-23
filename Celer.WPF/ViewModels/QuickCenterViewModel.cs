using ByteSizeLib;
using Celer.Infrastructure.Battery;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Threading;

namespace Celer.ViewModels
{
	public partial class QuickCenterViewModel : ObservableObject
	{
		private readonly string recycleBinUserPath = $"C:\\$Recycle.Bin\\{WindowsIdentity.GetCurrent().User}\\";

		private readonly Battery batteryService;

		private readonly DispatcherTimer _updateTimer = new()
		{
			Interval = TimeSpan.FromSeconds(2),
		};

		[ObservableProperty]
		public partial bool HasBattery { get; set; } = false;

		[ObservableProperty]
		public partial BatteryDevice? CurrentBattery { get; set; }

		[ObservableProperty]
		public partial bool CanCleanRecycleBin { get; set; } = false;

		[ObservableProperty]
		public partial bool IsCleaningRecycleBin { get; set; } = false;

		[ObservableProperty]
		public partial double RecycleBinTotalSize { get; set; }

		[ObservableProperty]
		public partial string RecycleBinReadableSize { get; set; }

		[ObservableProperty]
		public partial double RecycleBinCleanedSize { get; set; }

		partial void OnRecycleBinTotalSizeChanged(double value)
		{
			RecycleBinReadableSize = ByteSize.FromBytes(value).ToString();
		}

		public QuickCenterViewModel()
		{
			batteryService = new Battery();
			batteryService.Update();
			if (batteryService.Batteries.Count > 0)
			{
				CurrentBattery = batteryService.Batteries[0];
				HasBattery = true;
			}
			_updateTimer.Tick += (_, _) => UpdateBatteryInfo();
			RecycleBinTotalSize = GetRecycleBinSize();
			if (RecycleBinTotalSize > 0)
				CanCleanRecycleBin = true;
		}

		private void UpdateBatteryInfo()
		{
			if (batteryService is not null)
			{
				batteryService.Update();
				CurrentBattery = batteryService.Batteries[0];
			}
		}

		public double GetRecycleBinSize()
		{
			var dirs = Directory.GetFiles(recycleBinUserPath);
			double recycleBinSize = 0.0;
			foreach (var file in dirs)
				recycleBinSize += new FileInfo(file).Length;
			return recycleBinSize;
		}

		[RelayCommand]
		public void UserCleanRecycleBin()
		{
			IsCleaningRecycleBin = true;
			Task.Run(() =>
			{
				CleanRecycleBin();
				Thread.Sleep(5000);
			});
			IsCleaningRecycleBin = false;
			CanCleanRecycleBin = false;
		}

		private void CleanRecycleBin()
		{
			DirectoryInfo dir = new(recycleBinUserPath);
			foreach (FileInfo fi in dir.GetFiles("*", SearchOption.AllDirectories))
			{
				RecycleBinCleanedSize += fi.Length;
				try
				{
					fi.Delete();
				}
				catch (IOException e)
				{
					Debug.WriteLine($"Failed to delete file: {e.Message}");
				}
			}
		}


		[RelayCommand]
		public static void CloseCeler()
		{
			App.Current.Shutdown();
		}
	}
}
