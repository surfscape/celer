using ByteSizeLib;
using Celer.Properties;
using Celer.Services;
using Celer.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using static Celer.Views.Pages.Settings.SettingsModuleCleaningViewModel;
using Path = System.IO.Path;

namespace Celer.ViewModels
{
	public partial class CleanEngine : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<CleanupCategory> categories = [];

		[ObservableProperty]
		public partial string TotalFreedText { get; set; } = string.Empty;

		[ObservableProperty]
		public partial object? SelectedItem { get; set; }

		[ObservableProperty]
		public partial bool CanClean { get; set; } = false;

		[ObservableProperty]
		public partial bool CanCleanChecked { get; set; } = false;

		public class LogBook()
		{
			public DateTime LogDate { get; set; } = DateTime.Now;
			public string LogEntry { get; set; } = string.Empty;
			public LogType LogType { get; set; } = LogType.Information;
		}

		public enum LogType
		{
			Information,
			Success,
			Caution,
			Error
		}

		public ObservableCollection<LogBook> LogEntries { get; } = [];

		public CleanEngine()
		{
			WeakReferenceMessenger.Default.Register<TriggerCleaningSignaturesUpdate>(this, (r, m) =>
			{
				CanClean = m.Value;
				LoadSignatures();
			});

		}
		private void OnCleanItemChanged(CleanupItem item)
		{
			int checkedItems = Categories.Count(cat => cat.Items.Any(item => item.IsChecked.Equals(true)));

			if (item.IsChecked || checkedItems >= 1)
			{
				CanCleanChecked = true;
			}
			else
			{
				CanCleanChecked = false;
			}

		}

		/* TODO: Current implementation of log contains heavy performance issues and will be replaced with a better implementation.
            There's two issues, the log collection does not have a limit and thus depending on the tasks it will grow huge and start weighting on the UI thread to render all the log items.
            Second issue is that there's no delay so the logs are sent directly to the UI which in cases where a lof of operations are running it will spam the dispatcher.
            One plan is to have two seperate logging systems, one for the UI that tracks: signature loading, exceptions, and started/finished tasks which should reduce the amount of logs sent to the UI.
            The second logging system will be internal and log everything so that it can then be saved into a log file for debugging purposes.
        */
		private void AddLog(LogType type, string message)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				LogEntries.Add(
					new LogBook { LogDate = DateTime.Now, LogEntry = message, LogType = type }
				);
			});
		}
		private void LoadSignatures()
		{
			foreach (var cleanItem in Categories)
			{
				foreach (var item in cleanItem.Items)
					item.PropertyChanged -= (_, _) => OnCleanItemChanged(item);
			}
			Categories.Clear();
			try
			{
				AddLog(LogType.Information, "Loading signatures...");
				if (CleaningSignatureManager.GetSignatures() != string.Empty)
				{
					ParseJson(CleaningSignatureManager.GetSignatures());
					foreach (var cleanItem in Categories)
					{
						foreach (var item in cleanItem.Items)
							item.PropertyChanged += (_, _) => OnCleanItemChanged(item);
					}
				}
				else
					CanClean = false;

			}
			catch (Exception e)
			{
				AddLog(LogType.Error, $"An error occurred when loading the signatures: {e.Message}");
				CanClean = false;
			}
		}

		private void ParseJson(string json)
		{
			var doc = JsonDocument.Parse(json);
			foreach (var cat in doc.RootElement.EnumerateObject())
			{
				var items = new ObservableCollection<CleanupItem>();
				foreach (var item in cat.Value.EnumerateArray())
				{
					var name = item.GetProperty("name").GetString();

					var requiredProcesses = new List<RequiredProcess>();
					if (item.TryGetProperty("requiredProcesses", out var reqProcArray))
					{
						foreach (var proc in reqProcArray.EnumerateArray())
						{
							requiredProcesses.Add(
								new RequiredProcess
								{
									Name = proc.GetProperty("name").GetString()!,
									CanTerminate = proc.TryGetProperty("canTerminate", out var ct) && ct.GetBoolean(),
								}
							);
						}
					}
					var actions = new List<Action>();
					if (item.TryGetProperty("actions", out var actionArray))
					{
						foreach (var action in actionArray.EnumerateArray())
							actions.Add(CreateAction(action));
					}
					else if (item.TryGetProperty("action", out var action))
					{
						actions.Add(CreateAction(action));
					}

					items.Add(
						new CleanupItem
						{
							Name = name!,
							Description = item.TryGetProperty("description", out JsonElement desc)
								? desc.GetString() ?? string.Empty
								: string.Empty,
							Actions = actions,
							RequiredProcesses = requiredProcesses,
							IsChecked = false,
						}
					);
				}
				Categories.Add(new CleanupCategory { Name = cat.Name, Items = items });
			}
			var specialItems = new ObservableCollection<CleanupItem>();
			var customPathsFiles = new ObservableCollection<string>(MainConfiguration.Default.CLEANENGINE_CustomPaths?.Cast<string>() ?? []);
			if (customPathsFiles.Count > 0)
				specialItems.Add(new CleanupItem
				{
					Name = "User Defined Files & Folders",
					Description = "Delete files and folders added to the custom paths of the cleaning module settings page",
					Actions = GetUserCustomFilesAndFolders(customPathsFiles),
					IsChecked = false,
				});
			specialItems.Add(new CleanupItem
			{
				Name = "Disk Cleanup",
				Description = "Runs disk cleanup on the Windows drive and removes left over Windows Update packages",
				Actions = [new() { Type = ActionType.Command, Command = "cleanmgr.exe /d C: /VERYLOWDISK" }],
				IsChecked = false,
			});
			Categories.Add(
					new CleanupCategory { Name = "Special", Items = specialItems }
			);
			AddLog(LogType.Success, "Signatures loaded sucessfully");
		}

		public static Action CreateAction(JsonElement action)
		{
			ActionType actionType = ActionType.Empty;
			if (action.GetProperty("type").ValueKind == JsonValueKind.String)
				actionType = GetActionTypeFromLegacyType(action.GetProperty("type").GetString()!);
			else
				actionType = (ActionType)action.GetProperty("type").GetInt16();

			return new Action
			{
				Type = actionType,
				Path = action.GetProperty("path").GetString(),
				Patterns = action.TryGetProperty("patterns", out var patArray)
							? patArray.EnumerateArray().Select(p => p.GetString()!).ToList()
							: [],
			};
		}

		/// <summary>
		/// Receives the old action type in string and returns it's counterpart in the new enum format. This is to keep compatibility with cleaning signatures that use the old string to dictate action types, it's recommended to use the new int format
		/// </summary>
		/// <param name="stringType">The action type in the old string format</param>
		/// <returns>The enum counterpart of the string action type</returns>
		public static ActionType GetActionTypeFromLegacyType(string stringType)
		{
			return stringType switch
			{
				"folder-content" => ActionType.FolderContent,
				"file" => ActionType.File,
				"content-pattern" => ActionType.ContentPattern,
				"command" => ActionType.Command,
				_ => ActionType.Empty,
			};
		}
		private static List<Action> GetUserCustomFilesAndFolders(ObservableCollection<string> paths)
		{
			var actionList = new List<Action>();
			foreach (var item in paths)
			{
				FileAttributes attr = File.GetAttributes($@"{item}");

				if (attr.HasFlag(FileAttributes.Directory))
					actionList.Add(new Action
					{
						Type = ActionType.FolderContent,
						Path = item,
					});
				else
					actionList.Add(new Action
					{
						Type = ActionType.File,
						Path = item,
					});
			}
			return actionList;
		}

		[RelayCommand]
		private async Task CleanAsync()
		{
			TotalFreedText = string.Empty;

			/* add only the selected items in the categories for cleaning */
			var selectedItems = Categories
				.SelectMany(c => c.Items)
				.Where(i => i.IsChecked)
				.ToList();

			if (selectedItems.Count == 0)
			{
				AddLog(
					 LogType.Caution,
					 "At least one item has to be checked to start cleaning"
				 );
				SaveLog();
				return;
			}

			var runningProcs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var process in Process.GetProcesses())
			{
				using (process)
				{
					try { runningProcs.Add(process.ProcessName); }
					catch (InvalidOperationException e)
					{
						Debug.WriteLine($"Failed to retrieve process: {e.Message}");
					}
				}
			}

			var toClose = new HashSet<string>();
			foreach (var item in selectedItems)
			{
				if (item.RequiredProcesses != null && item.RequiredProcesses.Count > 0)
				{
					foreach (var proc in item.RequiredProcesses)
					{
						if (!proc.CanTerminate && runningProcs.Contains(Path.GetFileNameWithoutExtension(proc.Name)))
							toClose.Add(proc.Name);
					}
				}
			}

			if (toClose.Count > 0)
			{
				AddLog(LogType.Error, $"The following application have to be closed to proceed with the cleaning process:\n ${string.Join("\n", toClose)}");
				SaveLog();
				return;
			}

			long totalFreed = 0;

			await Task.Run(async () =>
			{
				CanClean = false;
				AddLog(LogType.Information, "Starting Celer Cleaning Engine...");

				foreach (var item in selectedItems)
				{
					foreach (var action in item.Actions)
					{
						if (action.Type == ActionType.FolderContent)
						{
							string resolvedPath = Environment.ExpandEnvironmentVariables(
								action.Path!
							);
							try
							{
								if (Directory.Exists(resolvedPath))
								{
									totalFreed += DeleteFolderContent(resolvedPath);
								}
								else
								{
									AddLog(
										 LogType.Error,
										 $"The folder {resolvedPath} does not exist or is invalid"
									 );
								}
							}
							catch (Exception ex)
							{
								AddLog(
									 LogType.Error,
									 $"Exception while trying to delete the folder {resolvedPath}: {ex.Message}"
								 );
							}
							continue;
						}

						if (action.Type == ActionType.ContentPattern)
						{
							string processHelper = string.Empty;
							string resolvedPath = Environment.ExpandEnvironmentVariables(
								action.Path!
							);
							try
							{
								if (Directory.Exists(resolvedPath))
								{
									foreach (var proc in item.RequiredProcesses ?? [])
									{
										if (proc.CanTerminate && proc.Name == "explorer.exe")
										{
											processHelper = proc.Name;
											Processes.KillExplorer();
											await Task.Delay(300);
										}
									}
									totalFreed += DeleteFilesWithPatterns(
										resolvedPath,
										action.Patterns!
									);
									if (processHelper == "explorer.exe")
										Processes.StartExplorer();
								}
								else
								{
									AddLog(
										LogType.Error,
										$"The folder {resolvedPath} does not exist or is invalid"
									);
								}
							}
							catch (Exception ex)
							{
								AddLog(
									 LogType.Error,
									 $"Exception while trying to delete the folder {resolvedPath} with content pattern: {ex.Message}"
								 );
							}
							continue;
						}
						if (action.Type == ActionType.Command && action.Command is not null)
						{
							var cDrive = new DriveInfo("C");
							long freeSpace = cDrive.TotalFreeSpace;
							AddLog(LogType.Information, $"Running command: {action.Command}");
							try
							{
								var startInfo = new ProcessStartInfo("powershell.exe", $"-Command {action.Command}")
								{
									RedirectStandardOutput = false, // currently the only command supported is cleanmgr which does not output anything to the console
									CreateNoWindow = true,
									//StandardOutputEncoding = Encoding.UTF8
								};

								using var process = new Process() { StartInfo = startInfo };
								process.Start();
								process.WaitForExit();

								if (action.Command.Contains("cleanmgr", StringComparison.OrdinalIgnoreCase))
								{
									Thread.Sleep(2000);
									// Cleanmgr launches it's own process which we don't have control over and as such we have to track if it's process is running or not to track it's lifecycle
									while (Process.GetProcessesByName("cleanmgr").Length > 0)
										Thread.Sleep(1000);
								}
							}
							catch (Exception e)
							{
								AddLog(LogType.Error, $"Failed to run the command \"{action.Command}\":\n{e.Message}");
							}
							finally
							{
								long finalFreeSpace = cDrive.TotalFreeSpace;
								totalFreed += Math.Max(0, finalFreeSpace - freeSpace);
							}
							continue;
						}
					}
					AddLog(LogType.Success, $"Task {item.Name} has finished");
				}
			});
			TotalFreedText = ByteSize.FromBytes(totalFreed).ToString();
			CanClean = true;
			SaveLog();
		}

		public void SaveLog()
		{
			if (MainConfiguration.Default.CLEANENGINE_ExportLog)
			{
				string logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				string logFileName = "CelerCleaningLog.txt";
				LogEntries.Add(
							new LogBook
							{
								LogDate = DateTime.Now,
								LogEntry = $"Saved to log file: {logFilePath}\\{logFileName}",
								LogType = LogType.Success,
							}
						);
				using StreamWriter outputFile = new(Path.Join(logFilePath, logFileName));
				foreach (LogBook logBook in LogEntries)
					outputFile.WriteLine($"{logBook.LogDate}: {logBook.LogEntry}");
			}
		}

		/// <summary>
		/// Deletes all files and folders in a specified directory recursively.
		/// </summary>
		/// <param name="resolvedPath">The path of the directory that we want to delete the contents of</param>
		public long DeleteFolderContent(string resolvedPath)
		{
			var dir = new DirectoryInfo(resolvedPath).GetDirectories("*", SearchOption.AllDirectories).OrderByDescending(d => d.FullName.Length);
			long folderSize = 0;
			foreach (var subDir in dir)
			{
				try
				{
					subDir.Attributes = FileAttributes.Normal;
					foreach (var file in subDir.GetFiles())
					{
						folderSize += file.Length;
					}
					subDir.Delete(true);
					AddLog(LogType.Success, $"Deleted folder {subDir.FullName}");
				}
				catch (Exception ex)
				{
					foreach (var file in subDir.GetFiles())
					{
						folderSize -= file.Length;
					}
					AddLog(
						 LogType.Error,
						 $"Exception when deleting folder {subDir.FullName}: {ex.Message}"
					 );
				}
			}
			return folderSize;
		}

		public long DeleteFilesWithPatterns(
			string resolvedPath,
			List<string> patterns
		)
		{
			long fileSize = 0;
			foreach (var pattern in patterns)
			{
				var files = Directory.GetFiles(resolvedPath, pattern, SearchOption.AllDirectories);
				foreach (var file in files)
				{
					var fileInfo = new FileInfo(file);
					try
					{
						fileSize += fileInfo.Length;
						fileInfo.Attributes = FileAttributes.Normal;
						fileInfo.Delete();
						AddLog(LogType.Success, $"Deleted the file {file}");
					}
					catch (Exception ex)
					{
						fileSize -= fileInfo.Length;
						AddLog(
							 LogType.Error,
							 $"Exception when deleting file {file}: {ex.Message}"
						 );
					}
				}
			}
			return fileSize;
		}

		public partial class CleanupItem : ObservableObject
		{
			public required string Name { get; set; }
			public string Description { get; set; } = string.Empty;
			public required List<Action> Actions { get; set; }
			public List<RequiredProcess>? RequiredProcesses { get; set; }

			[ObservableProperty]
			public partial bool IsChecked { get; set; } = false;
		}

		public class RequiredProcess
		{
			public required string Name { get; set; }
			public bool CanTerminate { get; set; }
		}

		public enum ActionType
		{
			FolderContent,
			File,
			ContentPattern,
			Command,
			Empty
		}

		public class Action
		{
			public required ActionType Type { get; set; }
			public string? Path { get; set; }
			public string? Command { get; set; }
			public List<string>? Patterns { get; set; }
		}

		public class CleanupCategory
		{
			public required string Name { get; set; }
			public required ObservableCollection<CleanupItem> Items { get; set; }
		}
	}
}
