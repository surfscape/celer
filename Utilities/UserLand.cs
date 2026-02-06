using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Celer.Utilities
{
    public class UserLand
    {
        public static bool IsInternetAvailable()
        {
            try
            {
                using Ping ping = new();
                PingReply reply = ping.Send("9.9.9.9", 2000); // TODO: add a preference to choose a different ping server if the default one is blocked in other countries
                return reply.Status == IPStatus.Success;
            }
            catch (PingException ex)
            {
                Debug.Write(ex.Message);
                return false;
            }
            catch (Exception ex) {
                Debug.Write(ex.Message);
                return false;
            }
        }

        public static void SetAutoStartup()
        {
            var process = Process.GetCurrentProcess();
            string fullPath = process.MainModule.FileName;
            using TaskService ts = new();
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "Run Celer as admin at startup";

            using LogonTrigger lt = new();
            lt.UserId = Environment.UserName;
            td.Triggers.Add(lt);

            using ExecAction ea = new(fullPath, "-silent", null);
            td.Actions.Add(ea);

            td.Settings.StartWhenAvailable = true;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;

            td.Principal.RunLevel = TaskRunLevel.Highest;

            ts.RootFolder.RegisterTaskDefinition(
                "Run Celer at Startup",
                td,
                TaskCreation.CreateOrUpdate,
                null,
                null,
                TaskLogonType.InteractiveToken
            );
            ts.GetTask("Run Celer at Startup").Enabled = true;
            Debug.WriteLine("Task created successfully!");
        }

        // TODO: currently only disabled the task, I should check to see if I can actually delete the task
        public static void RemoveAutoStartup()
        {
            using TaskService ts = new();
            Microsoft.Win32.TaskScheduler.Task task = ts.GetTask("Run Celer at Startup");

            if (task != null)
            {
                task.Enabled = false;
                Debug.WriteLine("Task disabled successfully!");
            }
            else
            {
                Debug.WriteLine("Task not found.");
            }
        }
    }
}
