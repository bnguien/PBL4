using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Models;
using Microsoft.Win32;
using System.Management;

namespace Client.Services
{
    public sealed class SystemInfoService
    {
        public async Task<SystemInfoModel> GetSystemInfoAsync(bool includeHardware, bool includeNetwork, bool includeSoftware)
        {
            return await Task.Run(() =>
            {
                var model = new SystemInfoModel
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Environment.MachineName,
                    Timestamp = DateTime.UtcNow
                };

                if (includeHardware)
                {
                    var hardware = new HardwareInfoModel();

                    // CPU via env + WMI for MaxClock
                    int maxClock = 0;
                    try
                    {
                        using var mcCpu = new ManagementObjectSearcher("select Name, MaxClockSpeed, NumberOfCores, NumberOfLogicalProcessors from Win32_Processor");
                        foreach (ManagementObject mo in mcCpu.Get())
                        {
                            maxClock = (mo["MaxClockSpeed"] is int i) ? i : 0;
                            var name = mo["Name"]?.ToString() ?? (Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown CPU");
                            var logical = mo["NumberOfLogicalProcessors"] is int l ? l : Environment.ProcessorCount;
                            var physical = mo["NumberOfCores"] is int p ? p : logical;
                            hardware.Cpu = new CpuInfoModel { Name = name, CoresLogical = logical, CoresPhysical = physical, MaxClockMHz = maxClock };
                            break;
                        }
                    }
                    catch
                    {
                        hardware.Cpu = new CpuInfoModel
                        {
                            Name = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Unknown CPU",
                            CoresLogical = Environment.ProcessorCount,
                            CoresPhysical = Environment.ProcessorCount,
                            MaxClockMHz = 0
                        };
                    }

                    // RAM total/available
                    try
                    {
                        // Total: use installed physical memory via WMI Win32_ComputerSystem.TotalPhysicalMemory (bytes)
                        using var mcOs = new ManagementObjectSearcher("select FreePhysicalMemory from Win32_OperatingSystem");
                        long freeBytes = 0;
                        foreach (ManagementObject mo in mcOs.Get())
                        {
                            if (mo["FreePhysicalMemory"] != null)
                            {
                                if (long.TryParse(mo["FreePhysicalMemory"].ToString(), out var kb))
                                {
                                    freeBytes = kb * 1024L;
                                }
                            }
                            break;
                        }
                        long totalBytes = 0;
                        using var mcCs = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem");
                        foreach (ManagementObject mo in mcCs.Get())
                        {
                            if (mo["TotalPhysicalMemory"] != null)
                            {
                                if (long.TryParse(mo["TotalPhysicalMemory"].ToString(), out var tb))
                                {
                                    totalBytes = tb;
                                }
                            }
                            break;
                        }
                        hardware.Ram = new RamInfoModel
                        {
                            TotalMB = totalBytes / (1024 * 1024),
                            AvailableMB = freeBytes / (1024 * 1024)
                        };
                    }
                    catch
                    {
                        hardware.Ram = new RamInfoModel
                        {
                            TotalMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024),
                            AvailableMB = 0
                        };
                    }

                    // Disks
                    hardware.Disks = DriveInfo.GetDrives()
                        .Where(d => d.IsReady)
                        .Select(d => new DiskInfoModel
                        {
                            DriveLetter = d.Name,
                            Type = d.DriveType.ToString(),
                            TotalBytes = d.TotalSize,
                            FreeBytes = d.AvailableFreeSpace,
                            Format = d.DriveFormat
                        })
                        .ToList();

                    // GPUs via WMI
                    try
                    {
                        using var mcGpu = new ManagementObjectSearcher("select Name, AdapterRAM from Win32_VideoController");
                        foreach (ManagementObject mo in mcGpu.Get())
                        {
                            var name = mo["Name"]?.ToString();
                            int? memMb = null;
                            if (mo["AdapterRAM"] != null && long.TryParse(mo["AdapterRAM"].ToString(), out var bytes))
                            {
                                memMb = (int)(bytes / (1024 * 1024));
                            }
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                hardware.Gpus.Add(new GpuInfoModel { Name = name, MemoryMB = memMb });
                            }
                        }
                    }
                    catch { }

                    model.Hardware = hardware;
                }

                if (includeNetwork)
                {
                    var adapters = NetworkInterface.GetAllNetworkInterfaces();
                    var ni = new NetworkInfoModel();
                    foreach (var a in adapters)
                    {
                        var props = a.GetIPProperties();
                        var adapter = new NetworkAdapterModel
                        {
                            Name = a.Name,
                            Mac = a.GetPhysicalAddress()?.ToString() ?? string.Empty,
                            IsUp = a.OperationalStatus == OperationalStatus.Up,
                            SpeedMbps = a.Speed > 0 ? a.Speed / 1_000_000 : null
                        };
                        foreach (var ua in props.UnicastAddresses)
                        {
                            var ip = ua.Address;
                            if (ip.IsIPv6LinkLocal) continue;
                            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                adapter.IPv4s.Add(ip.ToString());
                            else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                                adapter.IPv6s.Add(ip.ToString());
                        }
                        ni.Adapters.Add(adapter);
                    }
                    // Choose a realistic LAN IP if available
                    ni.PrimaryIPv4 = ni.Adapters.SelectMany(x => x.IPv4s)
                        .FirstOrDefault(ip => ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172.16."))
                        ?? ni.Adapters.SelectMany(x => x.IPv4s).FirstOrDefault()
                        ?? string.Empty;
                    ni.PrimaryMac = ni.Adapters.FirstOrDefault()?.Mac ?? string.Empty;
                    model.Network = ni;
                }

                if (includeSoftware)
                {
                    model.Software = new SoftwareInfoModel
                    {
                        OS = new OSInfoModel
                        {
                            Name = RuntimeInformation.OSDescription,
                            Version = Environment.OSVersion.Version.ToString(),
                            Build = Environment.OSVersion.Version.Build.ToString(),
                            Architecture = RuntimeInformation.OSArchitecture.ToString()
                        },
                        Runtime = new RuntimeInfoModel
                        {
                            DotnetVersion = Environment.Version.ToString(),
                            RuntimeDescription = RuntimeInformation.FrameworkDescription
                        },
                        InstalledApps = GetInstalledApps()
                    };
                }

                return model;
            });
        }

        private static System.Collections.Generic.List<InstalledAppModel> GetInstalledApps()
        {
            var results = new System.Collections.Generic.List<InstalledAppModel>();
            void ReadFromKey(RegistryKey? baseKey)
            {
                if (baseKey == null) return;
                foreach (var subName in baseKey.GetSubKeyNames())
                {
                    try
                    {
                        using var sub = baseKey.OpenSubKey(subName);
                        if (sub == null) continue;
                        var name = sub.GetValue("DisplayName") as string;
                        if (string.IsNullOrWhiteSpace(name)) continue; // skip non-display entries
                        var app = new InstalledAppModel
                        {
                            Name = name ?? string.Empty,
                            Version = sub.GetValue("DisplayVersion") as string,
                            Publisher = sub.GetValue("Publisher") as string,
                            InstallDate = sub.GetValue("InstallDate") as string
                        };
                        results.Add(app);
                    }
                    catch { }
                }
            }

            // HKLM 64-bit
            try { using var k1 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"); ReadFromKey(k1); } catch { }
            // HKLM 32-bit on x64
            try { using var k2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"); ReadFromKey(k2); } catch { }
            // HKCU
            try { using var k3 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"); ReadFromKey(k3); } catch { }

            // Deduplicate by Name+Version
            var distinct = results
                .GroupBy(a => (a.Name ?? string.Empty, a.Version ?? string.Empty))
                .Select(g => g.First())
                .OrderBy(a => a.Name)
                .ToList();
            return distinct;
        }
    }
}


