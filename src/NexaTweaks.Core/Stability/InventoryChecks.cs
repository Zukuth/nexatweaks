using System.Globalization;
using Microsoft.Win32;

namespace NexaTweaks.Core.Stability;

internal static class WmiValues
{
    public static string Text(IReadOnlyDictionary<string, object?> row, string name) =>
        row.TryGetValue(name, out var value) ? value?.ToString()?.Trim() ?? "" : "";

    public static string Day(DateTime date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>Parses the date part of a CIM_DATETIME ("yyyyMMddHHmmss.ffffff+UUU").</summary>
    public static DateTime? CimDate(IReadOnlyDictionary<string, object?> row, string name)
    {
        var text = Text(row, name);
        return text.Length >= 8 &&
               DateTime.TryParseExact(text[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }
}

/// <summary>Memory modules of different vendor/model or speed running together.</summary>
public sealed class MixedRamCheck : IStabilityCheck
{
    public string Id => "ram-mixed";
    public string Name => "Módulos de RAM";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var modules = probe.QueryWmi("Win32_PhysicalMemory", null,
            "BankLabel", "Manufacturer", "PartNumber", "Capacity", "Speed");
        if (modules.Count < 2) yield break;

        var models = modules.Select(m => $"{WmiValues.Text(m, "Manufacturer")}|{WmiValues.Text(m, "PartNumber")}")
            .Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var speeds = modules.Select(m => WmiValues.Text(m, "Speed")).Distinct().Count();
        if (models == 1 && speeds == 1) yield break;

        var evidence = modules.Select(m =>
        {
            var gb = double.TryParse(WmiValues.Text(m, "Capacity"), out var bytes) ? bytes / (1024d * 1024 * 1024) : 0;
            return $"{WmiValues.Text(m, "BankLabel")}: {WmiValues.Text(m, "Manufacturer")} {WmiValues.Text(m, "PartNumber")}, " +
                   $"{gb:0} GB, {WmiValues.Text(m, "Speed")} MHz";
        }).ToList();

        yield return new StabilityFinding(Id, "Hay módulos de RAM de distinto modelo o velocidad",
            StabilitySeverity.Medium, evidence,
            "Mezclar memorias funciona casi siempre, pero es una causa frecuente de pantallazos con códigos variados. " +
            "Ejecuta mdsched y, si hay fallos, prueba el equipo con un solo módulo o usa un kit idéntico.");
    }
}

/// <summary>Vendor display and network drivers older than three years.</summary>
public sealed class OldDriversCheck : IStabilityCheck
{
    private static readonly string[] Classes = { "DISPLAY", "NET" };

    public string Id => "old-drivers";
    public string Name => "Drivers antiguos";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var cutoff = now.AddYears(-3);
        var old = probe
            .QueryWmi("Win32_PnPSignedDriver", "DeviceClass='DISPLAY' OR DeviceClass='NET'",
                "DeviceClass", "DeviceName", "DriverProviderName", "Manufacturer", "DriverVersion", "DriverDate")
            .Where(d => Classes.Contains(WmiValues.Text(d, "DeviceClass"), StringComparer.OrdinalIgnoreCase))
            .Where(d => !IsMicrosoft(d))
            .Select(d => (Row: d, Date: WmiValues.CimDate(d, "DriverDate")))
            .Where(d => d.Date is not null && d.Date < cutoff)
            .OrderBy(d => d.Date)
            .ToList();
        if (old.Count == 0) yield break;

        yield return new StabilityFinding(Id, $"{old.Count} drivers de vídeo o red tienen más de 3 años",
            StabilitySeverity.Low,
            old.Select(d => $"{WmiValues.Text(d.Row, "DeviceName")} — versión {WmiValues.Text(d.Row, "DriverVersion")} " +
                            $"del {WmiValues.Day(d.Date!.Value)}").ToList(),
            "Descarga los drivers actuales desde la web de soporte de tu equipo o del fabricante del chip (AMD, NVIDIA, Intel, Realtek).");
    }

    private static bool IsMicrosoft(IReadOnlyDictionary<string, object?> driver) =>
        WmiValues.Text(driver, "DriverProviderName").Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
        WmiValues.Text(driver, "Manufacturer").Contains("Microsoft", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Firmware older than two years; BIOS updates fix fTPM, memory and power bugs.</summary>
public sealed class OldBiosCheck : IStabilityCheck
{
    public string Id => "old-bios";
    public string Name => "BIOS";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var bios = probe.QueryWmi("Win32_BIOS", null, "SMBIOSBIOSVersion", "Manufacturer", "ReleaseDate").FirstOrDefault();
        if (bios is null) yield break;

        var date = WmiValues.CimDate(bios, "ReleaseDate");
        if (date is null || date >= now.AddYears(-2)) yield break;

        yield return new StabilityFinding(Id, $"La BIOS es de {date:yyyy} y puede haber una versión más nueva",
            StabilitySeverity.Low,
            new[] { $"Versión {WmiValues.Text(bios, "SMBIOSBIOSVersion")} del {WmiValues.Day(date.Value)}" },
            "Busca actualizaciones de BIOS en la web de soporte de tu modelo (o en su app, como MyASUS). " +
            "Conecta el cargador y no apagues el equipo durante la actualización.");
    }
}

/// <summary>Third-party "optimized" Windows builds identify themselves in the registered owner.</summary>
public sealed class ModdedWindowsCheck : IStabilityCheck
{
    private const string CurrentVersionKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private static readonly string[] KnownMods =
    {
        "WinterOS", "ReviOS", "AtlasOS", "Tiny11", "Tiny10", "Ghost Spectre", "GhostSpectre",
        "X-Lite", "PhoenixLite", "KernelOS", "ExTiny", "Superlite",
    };

    public string Id => "modded-windows";
    public string Name => "Windows modificado";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var owner = probe.GetRegistryValue(RegistryHive.LocalMachine, CurrentVersionKey, "RegisteredOwner") as string ?? "";
        var organization = probe.GetRegistryValue(RegistryHive.LocalMachine, CurrentVersionKey, "RegisteredOrganization") as string ?? "";

        var mod = KnownMods.FirstOrDefault(m =>
            owner.Contains(m, StringComparison.OrdinalIgnoreCase) ||
            organization.Contains(m, StringComparison.OrdinalIgnoreCase));
        if (mod is null) yield break;

        yield return new StabilityFinding(Id, $"Este Windows es una versión modificada ({mod})",
            StabilitySeverity.Medium,
            new[] { $"RegisteredOwner = \"{owner}\"", $"RegisteredOrganization = \"{organization}\"" },
            "Estas versiones quitan componentes y cambian políticas de Windows, y son una causa habitual de inestabilidad. " +
            "Si los fallos siguen tras actualizar drivers y BIOS, instala Windows original con la Media Creation Tool de Microsoft.");
    }
}

/// <summary>Kernel drivers from anti-cheat and tuning tools that commonly show up in crash dumps.</summary>
public sealed class RiskyKernelDriversCheck : IStabilityCheck
{
    private static readonly (string Prefix, string Product)[] Known =
    {
        ("vgk", "Riot Vanguard (anti-trampas de Valorant/LoL)"),
        ("AMDRyzenMasterDriver", "AMD Ryzen Master (overclock)"),
        ("WinRing0", "WinRing0 (usado por herramientas de monitoreo y overclock)"),
        ("RTCore64", "MSI Afterburner / RivaTuner"),
    };

    public string Id => "kernel-drivers";
    public string Name => "Drivers de núcleo delicados";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var running = probe.QueryWmi("Win32_SystemDriver", "State='Running'", "Name", "State")
            .Where(d => string.Equals(WmiValues.Text(d, "State"), "Running", StringComparison.OrdinalIgnoreCase))
            .Select(d => WmiValues.Text(d, "Name"));

        var evidence = running
            .Select(name => (Name: name, Match: Known.FirstOrDefault(k => IsMatch(name, k.Prefix))))
            .Where(d => d.Match.Prefix is not null)
            .Select(d => $"{d.Name} — {d.Match.Product}")
            .ToList();
        if (evidence.Count == 0) yield break;

        yield return new StabilityFinding(Id, "Hay drivers de núcleo que suelen aparecer en pantallazos azules",
            StabilitySeverity.Low, evidence,
            "No son un fallo por sí mismos. Si los pantallazos continúan, desinstala temporalmente el programa " +
            "que instaló cada driver y comprueba si dejan de ocurrir.");
    }

    private static bool IsMatch(string name, string prefix) =>
        prefix == "vgk"
            ? string.Equals(name, prefix, StringComparison.OrdinalIgnoreCase)
            : name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
}
