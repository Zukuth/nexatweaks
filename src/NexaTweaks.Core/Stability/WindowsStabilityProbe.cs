using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Management;
using Microsoft.Win32;

namespace NexaTweaks.Core.Stability;

/// <summary>Reads the live event log (structured XPath query), registry and WMI.</summary>
public sealed class WindowsStabilityProbe : IStabilityProbe
{
    public IReadOnlyList<LogEvent> ReadEvents(string logName, IReadOnlyCollection<string> providers,
        IReadOnlyCollection<int> ids, DateTime since)
    {
        var query = new EventLogQuery(logName, PathType.LogName, BuildXPath(providers, ids, since))
        {
            ReverseDirection = true,
        };

        var events = new List<LogEvent>();
        using var reader = new EventLogReader(query);
        for (var record = reader.ReadEvent(); record is not null; record = reader.ReadEvent())
        {
            using (record)
            {
                events.Add(new LogEvent(
                    record.TimeCreated ?? DateTime.MinValue,
                    record.ProviderName,
                    record.Id,
                    record.Properties.Select(p => p.Value).ToList()));
            }
        }
        return events;
    }

    public object? GetRegistryValue(RegistryHive hive, string subKey, string valueName)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(subKey, writable: false);
        return key?.GetValue(valueName);
    }

    public IReadOnlyList<IReadOnlyDictionary<string, object?>> QueryWmi(string className, string? where,
        params string[] properties)
    {
        var select = properties.Length == 0 ? "*" : string.Join(", ", properties);
        var wql = $"SELECT {select} FROM {className}" + (where is null ? "" : $" WHERE {where}");

        var rows = new List<IReadOnlyDictionary<string, object?>>();
        using var searcher = new ManagementObjectSearcher(wql);
        using var results = searcher.Get();
        foreach (var obj in results)
        {
            using (obj)
            {
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var property in obj.Properties)
                    row[property.Name] = property.Value;
                rows.Add(row);
            }
        }
        return rows;
    }

    internal static string BuildXPath(IReadOnlyCollection<string> providers, IReadOnlyCollection<int> ids, DateTime since)
    {
        var conditions = new List<string>();
        if (providers.Count > 0)
            conditions.Add("(" + string.Join(" or ", providers.Select(p => $"Provider[@Name='{p}']")) + ")");
        conditions.Add("(" + string.Join(" or ", ids.Select(id => $"EventID={id}")) + ")");

        var sinceUtc = since.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        conditions.Add($"TimeCreated[@SystemTime>='{sinceUtc}']");

        return $"*[System[{string.Join(" and ", conditions)}]]";
    }
}
