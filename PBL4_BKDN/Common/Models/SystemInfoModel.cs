using System;
using System.Collections.Generic;

namespace Common.Models
{
    public sealed class SystemInfoModel
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }

        public HardwareInfoModel? Hardware { get; set; }
        public NetworkInfoModel? Network { get; set; }
        public SoftwareInfoModel? Software { get; set; }
    }

    public sealed class HardwareInfoModel
    {
        public CpuInfoModel? Cpu { get; set; }
        public List<GpuInfoModel> Gpus { get; set; } = new List<GpuInfoModel>();
        public RamInfoModel? Ram { get; set; }
        public List<DiskInfoModel> Disks { get; set; } = new List<DiskInfoModel>();
    }

    public sealed class CpuInfoModel
    {
        public string Name { get; set; } = string.Empty;
        public int CoresLogical { get; set; }
        public int CoresPhysical { get; set; }
        public int MaxClockMHz { get; set; }
        public double? UsagePercent { get; set; }
    }

    public sealed class GpuInfoModel
    {
        public string Name { get; set; } = string.Empty;
        public int? MemoryMB { get; set; }
    }

    public sealed class RamInfoModel
    {
        public long TotalMB { get; set; }
        public long AvailableMB { get; set; }
    }

    public sealed class DiskInfoModel
    {
        public string DriveLetter { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public string? Format { get; set; }
    }

    public sealed class NetworkInfoModel
    {
        public string PrimaryIPv4 { get; set; } = string.Empty;
        public string PrimaryMac { get; set; } = string.Empty;
        public List<NetworkAdapterModel> Adapters { get; set; } = new List<NetworkAdapterModel>();
    }

    public sealed class NetworkAdapterModel
    {
        public string Name { get; set; } = string.Empty;
        public List<string> IPv4s { get; set; } = new List<string>();
        public List<string> IPv6s { get; set; } = new List<string>();
        public string Mac { get; set; } = string.Empty;
        public bool IsUp { get; set; }
        public long? SpeedMbps { get; set; }
    }

    public sealed class SoftwareInfoModel
    {
        public OSInfoModel? OS { get; set; }
        public RuntimeInfoModel? Runtime { get; set; }
        public List<InstalledAppModel> InstalledApps { get; set; } = new List<InstalledAppModel>();
        public List<ProcessInfoModel>? TopProcesses { get; set; }
    }

    public sealed class OSInfoModel
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Build { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
    }

    public sealed class RuntimeInfoModel
    {
        public string DotnetVersion { get; set; } = string.Empty;
        public string RuntimeDescription { get; set; } = string.Empty;
    }

    public sealed class InstalledAppModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Version { get; set; }
        public string? Publisher { get; set; }
        public string? InstallDate { get; set; }
    }

    public sealed class ProcessInfoModel
    {
        public int Pid { get; set; }
        public string Name { get; set; } = string.Empty;
        public double CpuPercent { get; set; }
        public long WorkingSetMB { get; set; }
    }
}


