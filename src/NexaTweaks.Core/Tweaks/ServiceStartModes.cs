using System.Management;

namespace NexaTweaks.Core.Tweaks;

/// <summary>
/// One WMI query for every service's start mode, cached briefly.
/// Asking per service meant two queries per card (availability + applied state) and ~54 WMI round
/// trips just to open the Servicios page, which took seconds to settle.
/// </summary>
public static class ServiceStartModes
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(5);
    private static readonly object Lock = new();

    private static Dictionary<string, string>? _cache;
    private static DateTime _loadedAt;

    /// <summary>Start mode of a service (Auto, Manual, Disabled...), or null if it doesn't exist here.</summary>
    public static string? Get(string serviceName)
    {
        var snapshot = Snapshot();
        return snapshot.TryGetValue(serviceName, out var mode) ? mode : null;
    }

    /// <summary>Call after changing a service so the next read sees the new value.</summary>
    public static void Invalidate()
    {
        lock (Lock) { _cache = null; }
    }

    public static IReadOnlyDictionary<string, string> Snapshot()
    {
        lock (Lock)
        {
            if (_cache is not null && DateTime.UtcNow - _loadedAt < Ttl) return _cache;

            var modes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, StartMode FROM Win32_Service");
                using var results = searcher.Get();
                foreach (var obj in results)
                {
                    using (obj)
                    {
                        var name = obj["Name"]?.ToString();
                        var mode = obj["StartMode"]?.ToString();
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(mode)) modes[name] = mode;
                    }
                }
            }
            catch
            {
                // WMI unavailable: report "unknown" rather than pretending every service is missing
                return _cache ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            _cache = modes;
            _loadedAt = DateTime.UtcNow;
            return _cache;
        }
    }
}
