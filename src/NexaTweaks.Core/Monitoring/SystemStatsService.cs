using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace NexaTweaks.Core.Monitoring;

public sealed record SystemStats(double CpuPercent, double RamPercent, double RamUsedGb, double RamTotalGb, double GpuPercent, double DiskFreeGb, double DiskTotalGb, double? CpuTempCelsius, double NetDownKbps = 0, double NetUpKbps = 0, double CpuMhz = 0);

public sealed class SystemStatsService : IDisposable
{
    private readonly PerformanceCounter _cpuCounter = new("Processor", "% Processor Time", "_Total");
    private readonly GpuUsageService _gpu = new();
    private readonly CpuTemperatureService _cpuTemp = new();

    private long _lastBytesReceived;
    private long _lastBytesSent;
    private DateTime _lastNetSample = DateTime.MinValue;

    // Current clock = nominal MHz x "% Processor Performance" (which goes above 100 on turbo).
    // Cheaper than asking WMI for CurrentClockSpeed on every tick.
    private readonly PerformanceCounter? _cpuPerfCounter = TryCreateCpuPerformanceCounter();
    private readonly int _nominalMhz = ReadNominalMhz();

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

        var (downKbps, upKbps) = SampleNetwork();

        return new SystemStats(
            Math.Clamp(cpu, 0, 100), Math.Clamp(ramPercent, 0, 100), usedGb, totalGb,
            Math.Clamp(gpu, 0, 100), diskFree, diskTotal, _cpuTemp.SampleCelsius(), downKbps, upKbps,
            SampleCpuMhz());
    }

    private double SampleCpuMhz()
    {
        if (_cpuPerfCounter is null || _nominalMhz <= 0) return _nominalMhz;
        try
        {
            return Math.Round(_nominalMhz * _cpuPerfCounter.NextValue() / 100);
        }
        catch
        {
            return _nominalMhz;
        }
    }

    private static PerformanceCounter? TryCreateCpuPerformanceCounter()
    {
        try
        {
            var counter = new PerformanceCounter("Processor Information", "% Processor Performance", "_Total");
            counter.NextValue();
            return counter;
        }
        catch
        {
            return null;
        }
    }

    private static int ReadNominalMhz()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return key?.GetValue("~MHz") is int mhz ? mhz : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Throughput across all active adapters, from the delta between two samples.</summary>
    private (double DownKbps, double UpKbps) SampleNetwork()
    {
        try
        {
            long received = 0, sent = 0;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus != OperationalStatus.Up) continue;
                if (nic.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
                var stats = nic.GetIPStatistics();
                received += stats.BytesReceived;
                sent += stats.BytesSent;
            }

            var now = DateTime.UtcNow;
            var seconds = (now - _lastNetSample).TotalSeconds;
            var first = _lastNetSample == DateTime.MinValue;
            var down = first || seconds <= 0 ? 0 : (received - _lastBytesReceived) / 1024d / seconds;
            var up = first || seconds <= 0 ? 0 : (sent - _lastBytesSent) / 1024d / seconds;

            _lastBytesReceived = received;
            _lastBytesSent = sent;
            _lastNetSample = now;

            return (Math.Max(0, down), Math.Max(0, up));
        }
        catch
        {
            return (0, 0);
        }
    }

    public void Dispose()
    {
        _cpuCounter.Dispose();
        _cpuPerfCounter?.Dispose();
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
