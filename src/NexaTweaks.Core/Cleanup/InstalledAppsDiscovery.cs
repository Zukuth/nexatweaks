using Microsoft.Win32;

namespace NexaTweaks.Core.Cleanup;

public sealed record InstalledAppEntry(string DisplayName, string? Publisher, string? Version, string UninstallString);

/// <summary>Enumerates installed programs the same way Windows' own "Apps &amp; features" does, via the Uninstall registry keys.</summary>
public static class InstalledAppsDiscovery
{
    private static readonly (RegistryHive Hive, string SubKey)[] UninstallKeys =
    {
        (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
    };

    public static List<InstalledAppEntry> Discover()
    {
        var results = new List<InstalledAppEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (hive, subKey) in UninstallKeys)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var uninstallKey = baseKey.OpenSubKey(subKey);
            if (uninstallKey is null) continue;

            foreach (var appKeyName in uninstallKey.GetSubKeyNames())
            {
                using var appKey = uninstallKey.OpenSubKey(appKeyName);
                if (appKey is null) continue;

                var displayName = appKey.GetValue("DisplayName") as string;
                var uninstallString = appKey.GetValue("UninstallString") as string;
                var isSystemComponent = (appKey.GetValue("SystemComponent") as int?) == 1;
                var parentKeyName = appKey.GetValue("ParentKeyName") as string;

                if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(uninstallString)) continue;
                if (isSystemComponent || !string.IsNullOrEmpty(parentKeyName)) continue;
                if (!seen.Add(displayName)) continue;

                results.Add(new InstalledAppEntry(
                    displayName,
                    appKey.GetValue("Publisher") as string,
                    appKey.GetValue("DisplayVersion") as string,
                    uninstallString));
            }
        }

        return results.OrderBy(a => a.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
