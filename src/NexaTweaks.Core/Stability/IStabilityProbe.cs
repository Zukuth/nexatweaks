using Microsoft.Win32;

namespace NexaTweaks.Core.Stability;

/// <summary>An event log entry reduced to what the checks read: event data comes positionally,
/// in the order the provider's manifest declares it.</summary>
public sealed record LogEvent(DateTime Time, string Provider, int Id, IReadOnlyList<object?> Properties);

/// <summary>
/// The only way the stability checks touch the machine, so every check can be tested with
/// recorded data instead of the live event log, registry and WMI.
/// </summary>
public interface IStabilityProbe
{
    /// <summary>Events from <paramref name="logName"/>, newest first. An empty
    /// <paramref name="providers"/> collection matches any provider.</summary>
    IReadOnlyList<LogEvent> ReadEvents(string logName, IReadOnlyCollection<string> providers,
        IReadOnlyCollection<int> ids, DateTime since);

    object? GetRegistryValue(RegistryHive hive, string subKey, string valueName);

    /// <summary>Rows of a WMI class (root\cimv2); <paramref name="where"/> is an optional WQL filter.</summary>
    IReadOnlyList<IReadOnlyDictionary<string, object?>> QueryWmi(string className, string? where,
        params string[] properties);
}

public interface IStabilityCheck
{
    string Id { get; }
    string Name { get; }
    IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now);
}
