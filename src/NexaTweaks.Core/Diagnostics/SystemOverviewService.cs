using System.Globalization;
using System.Management;
using Microsoft.Win32;

namespace NexaTweaks.Core.Diagnostics;

/// <summary>Reads the machine description shown on the dashboard. WMI is slow, so callers should
/// run <see cref="Get"/> off the UI thread; the values don't change while the app is open.</summary>
public static class SystemOverviewService
{
    public static SystemOverview Get() => new(
        CpuName: First("Win32_Processor", "Name") ?? "CPU desconocida",
        GpuName: GpuName(),
        OsName: OsName(),
        Motherboard: $"{First("Win32_BaseBoard", "Manufacturer")} {First("Win32_BaseBoard", "Product")}".Trim(),
        RamModules: RamModules(),
        WindowsBadges: SystemOverviewFactory.WindowsBadges(
            VbsRunning(), HvciRunning(), RegistryString("DisplayVersion"), BuildNumber()),
        BoardBadges: SystemOverviewFactory.BoardBadges(
            TpmVersion(), SecureBootEnabled(), First("Win32_BIOS", "SMBIOSBIOSVersion") ?? "", BiosDate()));

    /// <summary>Edition only - the version and build ride along as badges.</summary>
    private static string OsName()
    {
        var product = RegistryString("ProductName");
        var name = string.IsNullOrWhiteSpace(product) ? "Windows" : product;
        // Windows 11 still reports "Windows 10 Pro" in ProductName; the build number is the truth.
        if (int.TryParse(BuildNumber().Split('.')[0], out var build) && build >= 22000)
            name = name.Replace("Windows 10", "Windows 11");
        return name;
    }

    private static string GpuName()
    {
        var names = Query("Win32_VideoController", "Name", "AdapterRAM")
            .OrderByDescending(r => r.TryGetValue("AdapterRAM", out var ram) && ram is not null
                ? Convert.ToInt64(ram, CultureInfo.InvariantCulture)
                : 0)
            .Select(r => r.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "")
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();
        return names.Count == 0 ? "GPU desconocida" : names[0];
    }

    private static IReadOnlyList<RamModuleInfo> RamModules() =>
        Query("Win32_PhysicalMemory", "BankLabel", "DeviceLocator", "Manufacturer", "Capacity", "Speed")
            .Select(r => SystemOverviewFactory.RamModule(
                slot: Text(r, "BankLabel") is { Length: > 0 } bank ? bank : Text(r, "DeviceLocator"),
                vendor: Text(r, "Manufacturer"),
                capacityBytes: ulong.TryParse(Text(r, "Capacity"), out var bytes) ? bytes : 0,
                speedMhz: int.TryParse(Text(r, "Speed"), out var speed) ? speed : 0))
            .ToList();

    private static bool VbsRunning() => DeviceGuard("VirtualizationBasedSecurityStatus") == 2;

    /// <summary>HVCI is service 2 in SecurityServicesRunning.</summary>
    private static bool HvciRunning()
    {
        try
        {
            var row = Query("Win32_DeviceGuard", "SecurityServicesRunning").FirstOrDefault();
            if (row is null || !row.TryGetValue("SecurityServicesRunning", out var value)) return false;
            return value is IEnumerable<object> items && items.Any(i => Convert.ToInt32(i, CultureInfo.InvariantCulture) == 2)
                   || value is uint[] raw && raw.Contains(2u);
        }
        catch
        {
            return false;
        }
    }

    private static int DeviceGuard(string property)
    {
        try
        {
            var row = Query("Win32_DeviceGuard", property).FirstOrDefault();
            return row is not null && row.TryGetValue(property, out var v) && v is not null
                ? Convert.ToInt32(v, CultureInfo.InvariantCulture)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string? TpmVersion()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                new ManagementScope(@"\\.\root\CIMV2\Security\MicrosoftTpm"),
                new ObjectQuery("SELECT SpecVersion, IsEnabled_InitialValue FROM Win32_Tpm"));
            using var results = searcher.Get();
            foreach (var obj in results)
            {
                using (obj)
                {
                    // SpecVersion looks like "2.0, 0, 1.38" - only the first part matters here.
                    var spec = obj["SpecVersion"]?.ToString()?.Split(',')[0]?.Trim();
                    if (!string.IsNullOrWhiteSpace(spec)) return spec;
                }
            }
        }
        catch
        {
            // no TPM, or the namespace needs elevation
        }
        return null;
    }

    private static bool? SecureBootEnabled()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            return key?.GetValue("UEFISecureBootEnabled") is int value ? value == 1 : null;
        }
        catch
        {
            return null;
        }
    }

    private static DateTime? BiosDate()
    {
        var raw = First("Win32_BIOS", "ReleaseDate") ?? "";
        return raw.Length >= 8 && DateTime.TryParseExact(raw[..8], "yyyyMMdd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    private static string BuildNumber()
    {
        var build = RegistryString("CurrentBuildNumber");
        var ubr = RegistryValue("UBR");
        return ubr is null ? build : $"{build}.{ubr}";
    }

    private static string RegistryString(string name) => RegistryValue(name)?.ToString() ?? "";

    private static object? RegistryValue(string name)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return key?.GetValue(name);
        }
        catch
        {
            return null;
        }
    }

    private static string Text(IReadOnlyDictionary<string, object?> row, string name) =>
        row.TryGetValue(name, out var value) ? value?.ToString()?.Trim() ?? "" : "";

    private static string? First(string className, string property) =>
        Query(className, property).FirstOrDefault() is { } row && row.TryGetValue(property, out var value)
            ? value?.ToString()?.Trim()
            : null;

    private static List<Dictionary<string, object?>> Query(string className, params string[] properties)
    {
        var rows = new List<Dictionary<string, object?>>();
        try
        {
            var scope = className == "Win32_DeviceGuard" ? @"\\.\root\Microsoft\Windows\DeviceGuard" : @"\\.\root\CIMV2";
            using var searcher = new ManagementObjectSearcher(
                new ManagementScope(scope),
                new ObjectQuery($"SELECT {string.Join(", ", properties)} FROM {className}"));
            using var results = searcher.Get();
            foreach (var obj in results)
            {
                using (obj)
                {
                    var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in properties) row[property] = SafeGet(obj, property);
                    rows.Add(row);
                }
            }
        }
        catch
        {
            // WMI unavailable - the dashboard degrades to "desconocido" instead of failing
        }
        return rows;
    }

    private static object? SafeGet(ManagementBaseObject obj, string property)
    {
        try { return obj[property]; }
        catch { return null; }
    }
}
