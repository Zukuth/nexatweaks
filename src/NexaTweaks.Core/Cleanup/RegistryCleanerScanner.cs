using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Cleanup;

public sealed record RegistryIssue(string Category, string Description, RegistryKeyDeleteTweak FixTweak);

/// <summary>
/// Conservative registry cleaner: only flags entries that reference a file that is already gone
/// from disk (orphaned App Paths, uninstall entries left over after someone deleted a program's
/// folder by hand). Deliberately narrow scope compared to a full CCleaner-style registry scan -
/// these categories can't realistically break anything, because what they point to is already gone.
/// </summary>
public static class RegistryCleanerScanner
{
    public static List<RegistryIssue> Scan()
    {
        var issues = new List<RegistryIssue>();
        ScanAppPaths(issues);
        ScanOrphanedUninstallEntries(issues);
        return issues;
    }

    private static void ScanAppPaths(List<RegistryIssue> issues)
    {
        const string parentSubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var parent = baseKey.OpenSubKey(parentSubKey);
        if (parent is null) return;

        foreach (var name in parent.GetSubKeyNames())
        {
            using var sub = parent.OpenSubKey(name);
            var path = (sub?.GetValue(null) as string)?.Trim('"');
            if (string.IsNullOrWhiteSpace(path) || File.Exists(path)) continue;

            issues.Add(new RegistryIssue(
                "Rutas de aplicación huérfanas",
                $"\"{name}\" apunta a un archivo que ya no existe: {path}",
                new RegistryKeyDeleteTweak
                {
                    Id = $"regclean.apppath.{name}",
                    Name = $"App Paths: {name}",
                    Description = $"Entrada huérfana que apuntaba a {path}",
                    Category = TweakCategory.Cleanup,
                    Risk = RiskLevel.Safe,
                    Hive = RegistryHive.LocalMachine,
                    ParentSubKey = parentSubKey,
                    KeyName = name,
                }));
        }
    }

    private static void ScanOrphanedUninstallEntries(List<RegistryIssue> issues)
    {
        var roots = new (RegistryHive Hive, string SubKey)[]
        {
            (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
        };

        foreach (var (hive, subKey) in roots)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var parent = baseKey.OpenSubKey(subKey);
            if (parent is null) continue;

            foreach (var name in parent.GetSubKeyNames())
            {
                using var sub = parent.OpenSubKey(name);
                if (sub is null) continue;

                var displayName = sub.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(displayName)) continue;

                var installLocation = sub.GetValue("InstallLocation") as string;
                var uninstallString = sub.GetValue("UninstallString") as string;

                var installLocationMissing = string.IsNullOrWhiteSpace(installLocation) || !Directory.Exists(installLocation);
                var uninstallTargetMissing = IsUninstallTargetMissing(uninstallString);

                if (!installLocationMissing || !uninstallTargetMissing) continue;

                issues.Add(new RegistryIssue(
                    "Programas desinstalados a medias",
                    $"\"{displayName}\" ya no está en el disco pero sigue apareciendo en Aplicaciones instaladas.",
                    new RegistryKeyDeleteTweak
                    {
                        Id = $"regclean.uninstall.{hive}.{name}",
                        Name = $"Entrada huérfana: {displayName}",
                        Description = "Quedó en el registro tras borrar la carpeta del programa manualmente, sin desinstalarlo.",
                        Category = TweakCategory.Cleanup,
                        Risk = RiskLevel.Safe,
                        Hive = hive,
                        ParentSubKey = subKey,
                        KeyName = name,
                    }));
            }
        }
    }

    private static bool IsUninstallTargetMissing(string? uninstallString)
    {
        if (string.IsNullOrWhiteSpace(uninstallString)) return true;

        var trimmed = uninstallString.Trim();

        // MSI-based installs (msiexec /X{guid}) aren't file paths - never second-guess those.
        if (trimmed.Contains("msiexec", StringComparison.OrdinalIgnoreCase)) return false;

        string path;
        if (trimmed.StartsWith('"'))
        {
            var end = trimmed.IndexOf('"', 1);
            path = end > 0 ? trimmed[1..end] : trimmed.Trim('"');
        }
        else
        {
            var spaceIdx = trimmed.IndexOf(" /", StringComparison.Ordinal);
            path = spaceIdx > 0 ? trimmed[..spaceIdx] : trimmed;
        }

        return !File.Exists(path);
    }
}
