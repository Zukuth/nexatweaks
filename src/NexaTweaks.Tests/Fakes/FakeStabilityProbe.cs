using Microsoft.Win32;
using NexaTweaks.Core.Stability;

namespace NexaTweaks.Tests.Fakes;

/// <summary>In-memory stand-in for the event log, registry and WMI so each stability check can be
/// fed exactly the data seen on a real machine.</summary>
public sealed class FakeStabilityProbe : IStabilityProbe
{
    private readonly List<(string Log, LogEvent Event)> _events = new();
    private readonly Dictionary<string, object> _registry = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Dictionary<string, object?>>> _wmi = new(StringComparer.OrdinalIgnoreCase);

    public FakeStabilityProbe AddEvent(string log, DateTime time, string provider, int id, params object?[] properties)
    {
        _events.Add((log, new LogEvent(time, provider, id, properties)));
        return this;
    }

    public FakeStabilityProbe SetRegistry(RegistryHive hive, string subKey, string name, object value)
    {
        _registry[Key(hive, subKey, name)] = value;
        return this;
    }

    public FakeStabilityProbe AddWmi(string className, Dictionary<string, object?> row)
    {
        if (!_wmi.TryGetValue(className, out var rows)) _wmi[className] = rows = new();
        rows.Add(row);
        return this;
    }

    public IReadOnlyList<LogEvent> ReadEvents(string logName, IReadOnlyCollection<string> providers,
        IReadOnlyCollection<int> ids, DateTime since) =>
        _events
            .Where(e => string.Equals(e.Log, logName, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Event)
            .Where(e => providers.Count == 0 || providers.Contains(e.Provider, StringComparer.OrdinalIgnoreCase))
            .Where(e => ids.Contains(e.Id) && e.Time >= since)
            .OrderByDescending(e => e.Time)
            .ToList();

    public object? GetRegistryValue(RegistryHive hive, string subKey, string valueName) =>
        _registry.TryGetValue(Key(hive, subKey, valueName), out var value) ? value : null;

    public IReadOnlyList<IReadOnlyDictionary<string, object?>> QueryWmi(string className, string? where,
        params string[] properties) =>
        _wmi.TryGetValue(className, out var rows)
            ? rows.Cast<IReadOnlyDictionary<string, object?>>().ToList()
            : Array.Empty<IReadOnlyDictionary<string, object?>>();

    private static string Key(RegistryHive hive, string subKey, string name) => $"{hive}\\{subKey}\\{name}";
}
