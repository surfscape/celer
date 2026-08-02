using Celer.Models.SystemInfo;
using Celer.Properties;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Celer.Services.Memory
{
    public partial class MemoryMonitorService
    { 
        private readonly float _memorySpeed; 
        private readonly List<RamSlotInfo> _ramSlots;

        private static readonly Lazy<Regex> _slotPatternRegex =
            new(() => new Regex(@"(\d+)", RegexOptions.Compiled));

        public MemoryMonitorService()
        {
            try
            {
                _memorySpeed = GetMemorySpeed();
                _ramSlots = GetRamSlotInfo();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing MemoryMonitorService: {ex.Message}");
                _memorySpeed = 0;
                _ramSlots = new List<RamSlotInfo>();
            }
        }

        public MemoryInfo GetMemoryInfo()
        {
            var (totalMB, usedMB, virtualTotal, virtualUsed) = GetMemory();

            return new MemoryInfo
            {
                UsedMemoryMB = usedMB,
                TotalMemoryMB = totalMB,
                SpeedMHz = _memorySpeed,
                VirtualUsedMB = MainConfiguration.Default.EnableRounding ? (int)virtualUsed : Math.Round(virtualUsed, 3),
                VirtualTotalMB = virtualTotal,
                Slots = _ramSlots,
            };
        }

        public static int GetMemorySpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Speed FROM Win32_PhysicalMemory"
                );
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>().Where(e => e["Speed"] is not null))
                {
                    return Convert.ToInt32(obj["Speed"]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to retrieve memory speed from WMI\n ${ex.Message}");
            }
            return 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public void RefreshLength() => dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
        }

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public static (double TotalMB, double UsedMB, double TotalVMB, double UsedVMB) GetMemory()
        {
            var memStatus = new MEMORYSTATUSEX();
            memStatus.RefreshLength();

            if (GlobalMemoryStatusEx(ref memStatus))
            {
                const double bytesInMB = 1024.0 * 1024.0;
                double totalMB = memStatus.ullTotalPhys / bytesInMB;
                double usedMB = (memStatus.ullTotalPhys - memStatus.ullAvailPhys) / bytesInMB;
                double totalVMB = memStatus.ullTotalPageFile / bytesInMB;
                double usedVMB = (memStatus.ullTotalPageFile - memStatus.ullAvailPageFile) / bytesInMB;

                return (Math.Round(totalMB, 2), Math.Round(usedMB, 2), Math.Round(totalVMB, 2), Math.Round(usedVMB, 2));
            }

            return (0, 0, 0, 0);
        }

        public static List<RamSlotInfo> GetRamSlotInfo()
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
                    foreach (ManagementObject array in arraySearcher.Get().Cast<ManagementObject>())
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
                    foreach (ManagementObject obj in memSearcher.Get().Cast<ManagementObject>())
                    {
                        var bankLabel = obj["BankLabel"] is not null ? Convert.ToString(obj["BankLabel"])! : "Undefined";
                        var deviceLocator = obj["DeviceLocator"] is not null ? Convert.ToString(obj["DeviceLocator"])! : "Undefined";

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
                        if (memoryTypeObj is not null)
                        {
                            memoryTypeStr = GetMemoryTypeString(Convert.ToInt32(memoryTypeObj));
                        }

                        string memoryFormFactor = "Unknown";
                        if (obj["FormFactor"] is not null)
                        {
                            memoryFormFactor = GetFormFactorString(
                                Convert.ToUInt16(obj["FormFactor"])
                            );
                        }

                        string manufacturer = obj["Manufacturer"] is not null ? Convert.ToString(obj["Manufacturer"])!.Trim() : "Unknown";
                        string model = obj["PartNumber"] is not null ? Convert.ToString(obj["PartNumber"])!.Trim() : "Unknown";

                        occupiedSlotsByParsedLabel[parsedSlotId] = new RamSlotInfo
                        {
                            IsOccupied = true,
                            Manufacturer = manufacturer,
                            Model = model,
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
        private static int ParseSlotNumber(string deviceLocator, string bankLabel)
        {
            string stringToParse = !string.IsNullOrWhiteSpace(deviceLocator)
                ? deviceLocator
                : bankLabel;

            if (string.IsNullOrWhiteSpace(stringToParse))
            {
                return -1;
            }

            MatchCollection matches = _slotPatternRegex.Value.Matches(stringToParse);
            if (matches.Count > 0)
            {
                string numStr = matches[^1].Groups[1].Value;
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
                1 => "Other",
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
