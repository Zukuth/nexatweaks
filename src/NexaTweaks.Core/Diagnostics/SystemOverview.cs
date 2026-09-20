using System.Globalization;

namespace NexaTweaks.Core.Diagnostics;

public enum BadgeTone
{
    Neutral,
    Good,
    Warn,
}

public sealed record SystemBadge(string Text, BadgeTone Tone);

public sealed record RamModuleInfo(string Slot, string Vendor, string CapacityText, string SpeedText);

/// <summary>Everything the dashboard shows about the machine that doesn't change while it runs.</summary>
public sealed record SystemOverview(
    string CpuName,
    string GpuName,
    string OsName,
    string Motherboard,
    IReadOnlyList<RamModuleInfo> RamModules,
    IReadOnlyList<SystemBadge> WindowsBadges,
    IReadOnlyList<SystemBadge> BoardBadges);

/// <summary>Turns raw system values into the labels shown on the dashboard cards.</summary>
public static class SystemOverviewFactory
{
    private const int BiosWarnAfterYears = 3;

    public static IReadOnlyList<SystemBadge> WindowsBadges(bool vbsEnabled, bool hvciEnabled,
        string displayVersion, string build)
    {
        var badges = new List<SystemBadge>
        {
            new(vbsEnabled ? "VBS activado" : "VBS desactivado", vbsEnabled ? BadgeTone.Good : BadgeTone.Warn),
            new(hvciEnabled ? "HVCI activado" : "HVCI desactivado", hvciEnabled ? BadgeTone.Good : BadgeTone.Warn),
        };

        if (!string.IsNullOrWhiteSpace(displayVersion))
            badges.Add(new SystemBadge(displayVersion.Trim(), BadgeTone.Neutral));
        if (!string.IsNullOrWhiteSpace(build))
            badges.Add(new SystemBadge($"Build {build.Trim()}", BadgeTone.Neutral));

        return badges;
    }

    public static IReadOnlyList<SystemBadge> BoardBadges(string? tpmVersion, bool? secureBoot,
        string biosVersion, DateTime? biosDate)
    {
        var badges = new List<SystemBadge>
        {
            string.IsNullOrWhiteSpace(tpmVersion)
                ? new SystemBadge("Sin TPM", BadgeTone.Warn)
                : new SystemBadge($"TPM {tpmVersion.Trim()}", BadgeTone.Good),
        };

        if (secureBoot is bool on)
            badges.Add(new SystemBadge(on ? "Secure Boot activado" : "Secure Boot desactivado",
                on ? BadgeTone.Good : BadgeTone.Warn));

        if (!string.IsNullOrWhiteSpace(biosVersion))
            badges.Add(new SystemBadge($"BIOS {biosVersion.Trim()}", BadgeTone.Neutral));

        if (biosDate is DateTime date)
        {
            var stale = date < DateTime.Now.AddYears(-BiosWarnAfterYears);
            badges.Add(new SystemBadge(date.Year.ToString(CultureInfo.InvariantCulture),
                stale ? BadgeTone.Warn : BadgeTone.Neutral));
        }

        return badges;
    }

    public static RamModuleInfo RamModule(string slot, string vendor, ulong capacityBytes, int speedMhz) =>
        new(
            Slot: string.IsNullOrWhiteSpace(slot) ? "DIMM" : slot.Trim(),
            Vendor: string.IsNullOrWhiteSpace(vendor) ? "Desconocido" : vendor.Trim(),
            CapacityText: $"{capacityBytes / 1024d / 1024 / 1024:0} GB",
            SpeedText: speedMhz > 0 ? $"{speedMhz} MT/s" : "");
}
