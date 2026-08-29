using System.Diagnostics;

namespace NexaTweaks.Core.Monitoring;

/// <summary>
/// Sums the "GPU Engine" performance counter category's 3D engine instances - the same
/// counters Task Manager's GPU graph is built on. Works across vendors (Nvidia/AMD/Intel)
/// without needing a vendor SDK.
/// </summary>
public sealed class GpuUsageService : IDisposable
{
    private List<PerformanceCounter>? _counters;
    private HashSet<string> _lastInstanceNames = new();
    private DateTime _lastCheck = DateTime.MinValue;

    public double Sample()
    {
        try
        {
            RefreshCountersIfNeeded();
            if (_counters is null || _counters.Count == 0) return 0;
            return _counters.Sum(c => SafeNextValue(c));
        }
        catch
        {
            return 0;
        }
    }

    private void RefreshCountersIfNeeded()
    {
        // GPU engine instances (one per running app using the GPU) rarely change - only pay the
        // cost of re-enumerating and rebuilding counters when the instance set actually differs,
        // instead of tearing everything down on a fixed timer regardless of whether anything moved.
        if (_counters is not null && DateTime.UtcNow - _lastCheck < TimeSpan.FromSeconds(10)) return;
        _lastCheck = DateTime.UtcNow;

        if (!PerformanceCounterCategory.Exists("GPU Engine"))
        {
            _counters ??= new List<PerformanceCounter>();
            return;
        }

        var category = new PerformanceCounterCategory("GPU Engine");
        var currentInstances = category.GetInstanceNames()
            .Where(i => i.Contains("engtype_3D", StringComparison.OrdinalIgnoreCase))
            .ToHashSet();

        if (_counters is not null && currentInstances.SetEquals(_lastInstanceNames)) return;

        _counters?.ForEach(c => c.Dispose());
        _counters = new List<PerformanceCounter>();

        foreach (var instance in currentInstances)
        {
            try
            {
                _counters.Add(new PerformanceCounter("GPU Engine", "Utilization Percentage", instance, true));
            }
            catch
            {
                // instance disappeared between enumeration and creation - skip it
            }
        }

        _lastInstanceNames = currentInstances;
    }

    private static double SafeNextValue(PerformanceCounter counter)
    {
        try { return counter.NextValue(); }
        catch { return 0; }
    }

    public void Dispose() => _counters?.ForEach(c => c.Dispose());
}
