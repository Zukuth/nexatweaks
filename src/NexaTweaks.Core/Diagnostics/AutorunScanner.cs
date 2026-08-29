using System.Diagnostics;
using System.Management;
using System.Text;
using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Diagnostics;

public sealed record AutorunEntry(
    string Source,
    string Name,
    string Command,
    bool IsMicrosoftSigned,
    ITweak DisableTweak);

/// <summary>
/// Autoruns-lite: enumerates the most common places Windows (and malware) auto-launch something
/// from - Run/RunOnce registry keys, scheduled tasks, and services set to start automatically -
/// so the user can see everything that starts with their PC in one place, not just Cleanup's
/// startup app list. Each entry can be disabled through the exact same reversible tweak types
/// used everywhere else in the app (StartupItemTweak, ScheduledTaskStateTweak, ServiceStateTweak).
/// </summary>
public static class AutorunScanner
{
    public static List<AutorunEntry> Scan()
    {
        var entries = new List<AutorunEntry>();
        try { ScanRunKeys(entries); } catch { /* best effort */ }
        try { ScanScheduledTasks(entries); } catch { /* best effort */ }
        try { ScanServices(entries); } catch { /* best effort */ }
        return entries;
    }

    private static void ScanRunKeys(List<AutorunEntry> entries)
    {
        var locations = new (RegistryHive Hive, string SubKey, string Label)[]
        {
            (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Inicio (equipo)"),
            (RegistryHive.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "Inicio (equipo, 32-bit)"),
            (RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "Inicio único (equipo)"),
            (RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Inicio (usuario)"),
            (RegistryHive.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", "Inicio único (usuario)"),
        };

        foreach (var (hive, subKey, label) in locations)
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(subKey);
            if (key is null) continue;

            foreach (var valueName in key.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(valueName)) continue;
                var command = key.GetValue(valueName) as string ?? "";
                var exePath = FileSignatureChecker.ExtractExecutablePath(command);

                entries.Add(new AutorunEntry(
                    label, valueName, command, FileSignatureChecker.IsMicrosoftSigned(exePath),
                    new StartupItemTweak
                    {
                        Id = $"autorun.run.{hive}.{subKey}.{valueName}",
                        Name = valueName,
                        Description = command,
                        Category = TweakCategory.Cleanup,
                        Risk = RiskLevel.Advanced,
                        Hive = hive,
                        SubKey = subKey,
                        ValueName = valueName,
                    }));
            }
        }
    }

    private static void ScanScheduledTasks(List<AutorunEntry> entries)
    {
        var psi = new ProcessStartInfo("schtasks.exe", "/query /fo csv /v")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var rows = ParseCsv(output);
        // Fixed schtasks CSV column order (locale-independent - only the header text is
        // translated, the column positions are not): 1=TaskName, 8=Task To Run, 11=Scheduled Task State.
        foreach (var row in rows.Skip(1))
        {
            if (row.Count < 12) continue;

            var taskName = row[1];
            var taskToRun = row[8];
            var state = row[11];

            if (string.IsNullOrWhiteSpace(taskName) || taskName == "N/A") continue;
            // "Scheduled Task State" is localized ("Enabled" / "Habilitado" on Spanish Windows) -
            // match on the common "Enabl"/"Habilita" stem so gender/language variants both work.
            if (!string.IsNullOrWhiteSpace(state) && !state.Contains("Enabl", StringComparison.OrdinalIgnoreCase)
                                                    && !state.Contains("Habilita", StringComparison.OrdinalIgnoreCase)) continue;

            var isSystemTask = taskName.StartsWith(@"\Microsoft\Windows\", StringComparison.OrdinalIgnoreCase);
            var exePath = FileSignatureChecker.ExtractExecutablePath(taskToRun);

            entries.Add(new AutorunEntry(
                "Tarea programada", taskName, taskToRun,
                isSystemTask || FileSignatureChecker.IsMicrosoftSigned(exePath),
                new ScheduledTaskStateTweak
                {
                    Id = $"autorun.task.{taskName}",
                    Name = taskName,
                    Description = taskToRun,
                    Category = TweakCategory.Cleanup,
                    Risk = RiskLevel.Advanced,
                    TaskPath = taskName,
                }));
        }
    }

    private static void ScanServices(List<AutorunEntry> entries)
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, DisplayName, PathName FROM Win32_Service WHERE StartMode='Auto'");

        foreach (ManagementObject mo in searcher.Get())
        {
            var name = mo["Name"]?.ToString() ?? "";
            var displayName = mo["DisplayName"]?.ToString() ?? name;
            var pathName = mo["PathName"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(name)) continue;

            var exePath = FileSignatureChecker.ExtractExecutablePath(pathName);

            entries.Add(new AutorunEntry(
                "Servicio (automático)", displayName, pathName,
                FileSignatureChecker.IsMicrosoftSigned(exePath),
                new ServiceStateTweak
                {
                    Id = $"autorun.service.{name}",
                    Name = displayName,
                    Description = pathName,
                    Category = TweakCategory.Cleanup,
                    Risk = RiskLevel.Risky,
                    ServiceName = name,
                    DesiredStartMode = "Disabled",
                }));
        }
    }

    /// <summary>Public so unit tests can exercise it directly against captured real schtasks output.</summary>
    public static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            rows.Add(ParseCsvLine(trimmed));
        }
        return rows;
    }

    public static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(current.ToString()); current.Clear(); }
                else current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields;
    }
}
