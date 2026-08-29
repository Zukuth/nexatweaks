using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

/// <summary>Enumerates HKCU/HKLM Run-key startup entries and wraps each as a StartupItemTweak.</summary>
public static class StartupItemDiscovery
{
    private static readonly (RegistryHive Hive, string SubKey)[] RunKeys =
    {
        (RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run"),
        (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"),
    };

    public static IReadOnlyList<StartupItemTweak> Discover()
    {
        var items = new List<StartupItemTweak>();

        foreach (var (hive, subKey) in RunKeys)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey);
            if (key is null) continue;

            foreach (var valueName in key.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(valueName)) continue;
                var command = key.GetValue(valueName) as string ?? "";

                items.Add(new StartupItemTweak
                {
                    Id = $"startup.{hive}.{valueName}",
                    Name = valueName,
                    Description = command,
                    Category = TweakCategory.Cleanup,
                    Risk = RiskLevel.Safe,
                    DefaultEnabled = false,
                    Hive = hive,
                    SubKey = subKey,
                    ValueName = valueName,
                });
            }
        }

        return items;
    }
}
