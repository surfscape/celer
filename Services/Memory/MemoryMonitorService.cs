using Celer.Models.SystemInfo;
using Celer.Properties;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Celer.Services.Memory
{
    public class MemoryMonitorService
    {
        public MemoryInfo GetMemoryInfo()
        {
            var (virtualTotal, virtualUsed) = GetVirtualMemory();

            return new MemoryInfo
            {
                UsedMemoryMB = GetUsedMemoryMB(),
                TotalMemoryMB = Math.Round(GetTotalMemory()),
                SpeedMHz = GetMemorySpeed(),
                VirtualUsedMB = MainConfiguration.Default.EnableRounding ? (int)virtualUsed : Math.Round(virtualUsed, 3),
                VirtualTotalMB = virtualTotal,
                Slots = GetRamSlotInfo(),
            };
        }

        // TODO implement a try-catch block since this can throw an exception if the PerformanceCounter is broken on the machine
        private static float GetUsedMemoryMB()
        {
            float availableMB = new PerformanceCounter("Memory", "Available MBytes").NextValue();
            double totalMB = GetTotalMemory();
            return (float)(totalMB - availableMB);
        }

        public static int? GetMemorySpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Speed FROM Win32_PhysicalMemory"
                );
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    if (obj["Speed"] != null)
                        return Convert.ToInt32(obj["Speed"]);
                }
            }
            catch
            {
                MessageBox.Show("Failed to retrieve memory speed");
            }
            return null;
        }

        private static double GetTotalMemory()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"
                );
                using var collection = searcher.Get();
                foreach (var item in collection)
                {
                    var result = (ulong)item["TotalVisibleMemorySize"];
                    return result / 1024.0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to obtain total memory: {ex.Message}");
            }
            return 0;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private static (double TotalMB, double UsedMB) GetVirtualMemory()
        {
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                double totalMB = memStatus.ullTotalPageFile / (1024.00 * 1024.00);
                double usedMB =
                    (memStatus.ullTotalPageFile - memStatus.ullAvailPageFile) / (1024.00 * 1024.00);
                return (totalMB, usedMB);
            }
            return (0, 0);
        }

        public List<RamSlotInfo> GetRamSlotInfo()
        {
            var slots = new List<RamSlotInfo>();

            try
            {
                int totalSlots = 0;
                using (
                    var arraySearcher = new ManagementObjectSearcher(
                        "SELECT MemoryDevices FROM Win32_PhysicalMemoryArray"
                    )
                )
                {
                    foreach (ManagementObject array in arraySearcher.Get())
                    {
                        totalSlots = Convert.ToInt32(array["MemoryDevices"]);
                        break;
                    }
                }

                if (totalSlots == 0)
                {
                    MessageBox.Show("Could not determine the RAM slots of the computer");
                }

                var occupiedSlotsByParsedLabel = new Dictionary<int, RamSlotInfo>();

                using (
                    var memSearcher = new ManagementObjectSearcher(
                        "SELECT BankLabel, Capacity, Manufacturer, PartNumber, MemoryType, FormFactor, SerialNumber, DeviceLocator, Tag FROM Win32_PhysicalMemory"
                    )
                )
                {
                    foreach (ManagementObject obj in memSearcher.Get())
                    {
                        var bankLabel = obj["BankLabel"] != null ? Convert.ToString(obj["BankLabel"]) : "Undefined";
                        var deviceLocator = obj["DeviceLocator"] != null ? Convert.ToString(obj["DeviceLocator"]) : "Undefined";

                        int parsedSlotId = ParseSlotNumber(deviceLocator, bankLabel);


                        if (parsedSlotId == -1)
                        {
                            Trace.WriteLine(
                                $"Failed to identify correct banklable and RAM slot: '{deviceLocator}', BankLabel: '{bankLabel}'. Ignoring module..."
                            );
                            continue;
                        }

                        var capacityMB =
                            obj["Capacity"] != null
                                ? Convert.ToUInt64(obj["Capacity"]) / (1024 * 1024)
                                : 0;

                        // Currently does not work for modern memory types like DDR4, DDR5, and laptops. I should move to using SMBIOS or a third party library like AIDA64 or CPUZ to get memory type and form factor
                        string memoryTypeStr = "Unknown";
                        var memoryTypeObj = obj["MemoryType"];
                        if (memoryTypeObj != null)
                        {
                            memoryTypeStr = GetMemoryTypeString(Convert.ToInt32(memoryTypeObj));
                        }

                        string memoryFormFactor = "Unknown";
                        if (obj["FormFactor"] != null)
                        {
                            memoryFormFactor = GetFormFactorString(
                                Convert.ToUInt16(obj["FormFactor"])
                            );
                        }

                        occupiedSlotsByParsedLabel[parsedSlotId] = new RamSlotInfo
                        {
                            IsOccupied = true,
                            Manufacturer = obj["Manufacturer"]?.ToString().Trim() ?? "Unknown",
                            Model = obj["PartNumber"]?.ToString().Trim() ?? "Unknown",
                            SizeMB = (int)capacityMB,
                            MemoryType = memoryTypeStr,
                            FormFactor = memoryFormFactor,
                            BankLabel = bankLabel,
                            DeviceLocator = deviceLocator,
                        };
                    }
                }

                bool isLikelyOneBased = false;
                if (occupiedSlotsByParsedLabel.Count != 0)
                {
                    int minParsedKey = occupiedSlotsByParsedLabel.Keys.Min();
                    if (minParsedKey == 1 && !occupiedSlotsByParsedLabel.ContainsKey(0))
                    {
                        isLikelyOneBased = true;
                    }
                }

                for (int i = 0; i < totalSlots; i++)
                {
                    int keyToLookup = i;
                    if (isLikelyOneBased)
                    {
                        keyToLookup = i + 1;
                    }

                    if (occupiedSlotsByParsedLabel.TryGetValue(keyToLookup, out var slotInfo))
                    {
                        slotInfo.SlotNumber = "Slot " + i;
                        slots.Add(slotInfo);
                    }
                    else
                    {
                        slots.Add(
                            new RamSlotInfo
                            {
                                SlotNumber = "Slot " + i,
                                IsOccupied = false,
                                Manufacturer = "",
                                Model = "",
                                SizeMB = 0,
                                MemoryType = "",
                                FormFactor = "",
                                BankLabel =
                                    $"Slot {i}{(isLikelyOneBased ? " (Expected Label " + (i + 1) + ")" : "")}",
                                DeviceLocator = $"Physical Slot {i}",
                            }
                        );
                    }
                }
            }
            catch (ManagementException mex)
            {
                Debug.WriteLine(
                    $"WMI Error obtaining RAM slot info: {mex.Message} (Error Code: {mex.ErrorCode})"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obtaining RAM slot info: {ex.Message}");
            }

            return slots;
        }

        /// <summary>
        /// This function is used to make a relationship between the RAM slot from WMI and the one from kernel32.dll
        /// </summary>
        /// <param name="deviceLocator">ID of the RAM stick</param>
        /// <param name="bankLabel">Bank where the RAM stick is in</param>
        /// <returns></returns>
        private int ParseSlotNumber(string deviceLocator, string bankLabel)
        {
            string S_SLOT_PATTERN = @"(\d+)";

            string stringToParse = !string.IsNullOrWhiteSpace(deviceLocator)
                ? deviceLocator
                : bankLabel;

            if (string.IsNullOrWhiteSpace(stringToParse))
            {
                return -1;
            }

            MatchCollection matches = Regex.Matches(stringToParse, S_SLOT_PATTERN);
            if (matches.Count > 0)
            {
                string numStr = matches[matches.Count - 1].Groups[1].Value;
                if (int.TryParse(numStr, out int slotNum))
                {
                    return slotNum;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the lable of the corresponding MemoryType ID from the RAM
        /// </summary>
        /// <param name="typeCode">The MemoryType ID</param>
        /// <returns></returns>
        private static string GetMemoryTypeString(int typeCode)
        {
            return typeCode switch
            {
                0 => "Not Supported",
                1 => "Other",
                2 => "DRAM",
                17 => "SDRAM",
                18 => "RDRAM",
                19 => "EDO",
                20 => "DDR",
                21 => "DDR2",
                22 => "DDR2 FB-DIMM",
                24 => "DDR3",
                25 => "FBD2",
                26 => "DDR4",
                27 => "LPDDR",
                28 => "LPDDR2",
                29 => "LPDDR3",
                30 => "LPDDR4",
                31 => "DDR5",
                32 => "LPDDR5",
                34 => "DDR5",
                _ => $"Uknown ({typeCode})",
            };
        }

        /// <summary>
        /// Returns the lable of the corresponding FormFactor ID from the RAM
        /// </summary>
        /// <param name="id">The FormFactor ID</param>
        /// <returns></returns>
        private static string GetFormFactorString(int id)
        {
            return id switch
            {
                0 => "Not Supported",
                1 => "Outro",
                2 => "SIP",
                3 => "DIP",
                4 => "ZIP",
                5 => "SOJ",
                6 => "Proprietary",
                7 => "SIMM",
                8 => "DIMM",
                9 => "TSOP",
                10 => "PGA",
                11 => "RIMM",
                12 => "SODIMM",
                13 => "SRIMM",
                14 => "SMD",
                15 => "SSMP",
                16 => "QFP",
                17 => "TQFP",
                18 => "SOIC",
                19 => "LCC",
                20 => "PLCC",
                21 => "BGA",
                22 => "FPBGA",
                23 => "LGA",
                _ => $"Uknown ({id})",
            };
        }
    }
}
