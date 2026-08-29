using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NexaTweaks.Core.Monitoring;

public sealed record SystemStats(double CpuPercent, double RamPercent, double RamUsedGb, double RamTotalGb, double GpuPercent, double DiskFreeGb, double DiskTotalGb, double? CpuTempCelsius);

public sealed class SystemStatsService : IDisposable
{
    private readonly PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
    private readonly GpuUsageService _gpu = new();
    private readonly CpuTemperatureService _cpuTemp = new();

    public SystemStatsService()
    {
        _cpuCounter.NextValue(); // first call always returns 0, warm it up
    }

    public SystemStats Sample()
    {
        var cpu = _cpuCounter.NextValue();

        var memStatus = new MEMORYSTATUSEX();
        GlobalMemoryStatusEx(memStatus);
        var totalGb = memStatus.ullTotalPhys / 1024d / 1024 / 1024;
        var availGb = memStatus.ullAvailPhys / 1024d / 1024 / 1024;
        var usedGb = totalGb - availGb;
        var ramPercent = totalGb > 0 ? usedGb / totalGb * 100 : 0;

        var gpu = _gpu.Sample();

        var systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
        double diskFree = 0, diskTotal = 0;
        try
        {
            var drive = new DriveInfo(systemDrive);
            diskFree = drive.AvailableFreeSpace / 1024d / 1024 / 1024;
            diskTotal = drive.TotalSize / 1024d / 1024 / 1024;
        }
        catch
        {
            // drive info unavailable - leave as zero
        }

        return new SystemStats(
            Math.Clamp(cpu, 0, 100), Math.Clamp(ramPercent, 0, 100), usedGb, totalGb,
            Math.Clamp(gpu, 0, 100), diskFree, diskTotal, _cpuTemp.SampleCelsius());
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
        _gpu.Dispose();
    }

    [DllImport("kernel32.dll")]
    private static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MEMORYSTATUSEX
    {
        public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }
}
