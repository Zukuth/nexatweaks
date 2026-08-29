using System.Management;
using Microsoft.Win32;

namespace NexaTweaks.Core.Diagnostics;

public sealed record SystemInfo(
    string OsName,
    string OsVersion,
    string CpuName,
    int CpuCores,
    int CpuLogicalProcessors,
    string GpuName,
    double RamTotalGb,
    string MotherboardName,
    string SystemType);

public static class SystemInfoService
{
    public static SystemInfo Get()
    {
        return new SystemInfo(
            OsName: GetOsName(),
            OsVersion: GetOsVersion(),
            CpuName: QueryFirst("Win32_Processor", "Name") ?? "Desconocido",
            CpuCores: int.TryParse(QueryFirst("Win32_Processor", "NumberOfCores"), out var cores) ? cores : 0,
            CpuLogicalProcessors: int.TryParse(QueryFirst("Win32_Processor", "NumberOfLogicalProcessors"), out var lp) ? lp : 0,
            GpuName: QueryFirst("Win32_VideoController", "Name") ?? "Desconocida",
            RamTotalGb: GetTotalRamGb(),
            MotherboardName: BuildMotherboardName(),
            SystemType: QueryFirst("Win32_OperatingSystem", "OSArchitecture") ?? "Desconocido");
    }

    private static string GetOsName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var productName = key?.GetValue("ProductName") as string;
            var displayVersion = key?.GetValue("DisplayVersion") as string;
            return string.IsNullOrEmpty(displayVersion) ? productName ?? "Windows" : $"{productName} {displayVersion}";
        }
        catch
        {
            return "Windows";
        }
    }

    private static string GetOsVersion()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var build = key?.GetValue("CurrentBuildNumber") as string;
            var ubr = key?.GetValue("UBR");
            return ubr is not null ? $"Build {build}.{ubr}" : $"Build {build}";
        }
        catch
        {
            return Environment.OSVersion.VersionString;
        }
    }

    private static double GetTotalRamGb()
    {
        var raw = QueryFirst("Win32_ComputerSystem", "TotalPhysicalMemory");
        if (raw is not null && double.TryParse(raw, out var bytes))
            return Math.Round(bytes / 1024 / 1024 / 1024, 1);
        return 0;
    }

    private static string BuildMotherboardName()
    {
        var manufacturer = QueryFirst("Win32_BaseBoard", "Manufacturer");
        var product = QueryFirst("Win32_BaseBoard", "Product");
        return string.IsNullOrWhiteSpace(manufacturer) && string.IsNullOrWhiteSpace(product)
            ? "Desconocida"
            : $"{manufacturer} {product}".Trim();
    }

    private static string? QueryFirst(string wmiClass, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {wmiClass}");
            foreach (ManagementObject mo in searcher.Get())
                return mo[property]?.ToString();
        }
        catch
        {
            // WMI class/property unavailable on this system
        }
        return null;
    }
}
