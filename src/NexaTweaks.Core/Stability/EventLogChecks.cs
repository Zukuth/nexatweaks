using System.Globalization;
using System.Text.RegularExpressions;

namespace NexaTweaks.Core.Stability;

internal static class EventLogSources
{
    public const string System = "System";
    public const string Application = "Application";
    public const string BugCheckProvider = "Microsoft-Windows-WER-SystemErrorReporting";
    public const int BugCheckId = 1001;

    public static IReadOnlyList<LogEvent> BugChecks(IStabilityProbe probe, DateTime since) =>
        probe.ReadEvents(System, new[] { BugCheckProvider }, new[] { BugCheckId }, since);

    public static string Stamp(DateTime time) => time.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
}

/// <summary>Blue screens (WER event 1001) of the last 30 days, grouped by stop code.</summary>
public sealed partial class BugCheckCheck : IStabilityCheck
{
    public string Id => "bugcheck";
    public string Name => "Pantallazos azules";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var crashes = EventLogSources.BugChecks(probe, now.AddDays(-30));
        if (crashes.Count == 0) yield break;

        var groups = crashes
            .GroupBy(e => ParseCode(e.Properties.FirstOrDefault()))
            .OrderByDescending(g => g.Count())
            .ToList();

        var evidence = groups.Select(g =>
            $"{FormatCode(g.Key)} {BugCheckNames.Get(g.Key)} ×{g.Count()} (último: {EventLogSources.Stamp(g.Max(e => e.Time))})")
            .ToList();

        var recommendation = groups.Count >= 3
            ? "Los códigos cambian en cada fallo: eso casi nunca es un solo driver. Revisa primero la RAM (mdsched o MemTest86), " +
              "quita cualquier overclock/undervolt y, si usas un Windows modificado, instala el Windows original."
            : "Actualiza los drivers de chipset y GPU desde la web del fabricante y ejecuta el test de memoria (mdsched). " +
              "Los volcados de C:\\Windows\\Minidump indican el driver exacto si los abres con WinDbg.";

        yield return new StabilityFinding(Id, $"{crashes.Count} pantallazos azules en los últimos 30 días",
            StabilitySeverity.High, evidence, recommendation);
    }

    private static uint? ParseCode(object? raw)
    {
        var match = HexCode().Match(raw?.ToString() ?? "");
        return match.Success && uint.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, null, out var code)
            ? code
            : null;
    }

    private static string FormatCode(uint? code) => code is null ? "0x?" : $"0x{code:X}";

    [GeneratedRegex(@"0x([0-9a-fA-F]{1,8})\b")]
    private static partial Regex HexCode();
}

/// <summary>Kernel-Power 41 events without a stop code: hard hangs, black screens or power cuts.</summary>
public sealed class UnexpectedShutdownCheck : IStabilityCheck
{
    public string Id => "unexpected-shutdown";
    public string Name => "Apagados inesperados";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var withoutStopCode = probe
            .ReadEvents(EventLogSources.System, new[] { "Microsoft-Windows-Kernel-Power" }, new[] { 41 }, now.AddDays(-30))
            .Where(e => Convert.ToInt64(e.Properties.FirstOrDefault() ?? 0L) == 0)
            .ToList();
        if (withoutStopCode.Count == 0) yield break;

        yield return new StabilityFinding(Id,
            $"{withoutStopCode.Count} cuelgues o apagados sin pantallazo azul en los últimos 30 días",
            StabilitySeverity.Medium,
            withoutStopCode.Select(e => $"Kernel-Power 41 el {EventLogSources.Stamp(e.Time)}").ToList(),
            "Windows se reinició sin poder registrar el motivo: suele ser un cuelgue total (pantalla negra), " +
            "un apagado forzado con el botón o un corte de energía. Si no apagaste tú el equipo, revisa temperaturas, " +
            "el cargador y los drivers de la GPU.");
    }
}

/// <summary>TPM event 14: the firmware TPM reported an unrecoverable error.</summary>
public sealed class TpmErrorCheck : IStabilityCheck
{
    public string Id => "tpm";
    public string Name => "Chip de seguridad (TPM)";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var errors = probe.ReadEvents(EventLogSources.System, new[] { "TPM" }, new[] { 14 }, now.AddDays(-30));
        if (errors.Count == 0) yield break;

        yield return new StabilityFinding(Id, "El TPM reporta un error de hardware irrecuperable",
            StabilitySeverity.High,
            new[] { $"TPM evento 14 ×{errors.Count} (último: {EventLogSources.Stamp(errors[0].Time)})" },
            "Actualiza la BIOS desde la web o la app del fabricante; en equipos AMD el fTPM se corrige con firmware nuevo. " +
            "Si persiste, restablece el TPM desde la BIOS (puede afectar a BitLocker: guarda antes la clave de recuperación).");
    }
}

/// <summary>WHEA-Logger events: the CPU, memory or PCIe bus reported hardware errors.</summary>
public sealed class HardwareErrorCheck : IStabilityCheck
{
    public string Id => "whea";
    public string Name => "Errores de hardware (WHEA)";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var errors = probe.ReadEvents(EventLogSources.System, new[] { "Microsoft-Windows-WHEA-Logger" },
            new[] { 17, 18, 19, 47 }, now.AddDays(-30));
        if (errors.Count == 0) yield break;

        var evidence = errors.GroupBy(e => e.Id)
            .Select(g => $"WHEA-Logger {g.Key} ×{g.Count()} (último: {EventLogSources.Stamp(g.Max(e => e.Time))})")
            .ToList();

        yield return new StabilityFinding(Id, "El hardware reportó errores (procesador, memoria o PCIe)",
            StabilitySeverity.High, evidence,
            "Quita cualquier overclock o undervolt, actualiza la BIOS y prueba la RAM. " +
            "Si siguen apareciendo, el hardware puede estar fallando.");
    }
}

/// <summary>Application Error 1000 in the last 7 days, grouped by app and faulting module.</summary>
public sealed class AppCrashCheck : IStabilityCheck
{
    private const int MinCrashes = 3;
    private const int MaxFindings = 5;

    public string Id => "app-crash";
    public string Name => "Aplicaciones que se cierran solas";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var groups = probe
            .ReadEvents(EventLogSources.Application, new[] { "Application Error" }, new[] { 1000 }, now.AddDays(-7))
            .GroupBy(e => (App: Prop(e, 0), Module: Prop(e, 3)))
            .Where(g => g.Count() >= MinCrashes)
            .OrderByDescending(g => g.Count())
            .Take(MaxFindings);

        foreach (var g in groups)
        {
            yield return new StabilityFinding(Id,
                $"{g.Key.App} se cerró {g.Count()} veces en 7 días",
                StabilitySeverity.Medium,
                new[] { $"Módulo con errores: {g.Key.Module} (último cierre: {EventLogSources.Stamp(g.Max(e => e.Time))})" },
                RecommendationFor(g.Key.App, g.Key.Module));
        }
    }

    private static string Prop(LogEvent e, int index) =>
        index < e.Properties.Count ? e.Properties[index]?.ToString() ?? "?" : "?";

    private static string RecommendationFor(string app, string module)
    {
        var m = module.ToLowerInvariant();
        var vendor = m.StartsWith("amd") || m.StartsWith("ati") ? "AMD (Adrenalin)"
            : m.StartsWith("nv") ? "NVIDIA"
            : m.StartsWith("ig") ? "Intel"
            : null;
        return vendor is not null
            ? $"El fallo está en {module}, que pertenece al driver de gráficos. Instala el driver actual de {vendor}; " +
              $"si {app} sigue fallando, actualízalo o desinstálalo."
            : $"Actualiza o reinstala {app}. Si el módulo {module} es de otro programa, actualiza también ese programa.";
    }
}

/// <summary>Windows Memory Diagnostic results (1101 = passed, 1102 = errors found).</summary>
public sealed class MemoryTestCheck : IStabilityCheck
{
    private const string Provider = "Microsoft-Windows-MemoryDiagnostics-Results";

    public string Id => "memory-test";
    public string Name => "Test de memoria RAM";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var results = probe.ReadEvents(EventLogSources.System, new[] { Provider }, new[] { 1101, 1102 }, now.AddDays(-365));
        var latest = results.MaxBy(e => e.Time);

        if (latest?.Id == 1102)
        {
            yield return new StabilityFinding(Id, "El test de memoria encontró errores en la RAM",
                StabilitySeverity.High,
                new[] { $"MemoryDiagnostics-Results 1102 el {EventLogSources.Stamp(latest.Time)}" },
                "Prueba cada módulo por separado (y en otra ranura) para identificar el defectuoso y reemplázalo.");
            yield break;
        }

        if (latest is null && EventLogSources.BugChecks(probe, now.AddDays(-30)).Count > 0)
        {
            yield return new StabilityFinding(Id, "Hay pantallazos azules y la RAM nunca se probó",
                StabilitySeverity.Medium,
                new[] { "Sin resultados de MemoryDiagnostics-Results en el último año" },
                "Ejecuta mdsched.exe y elige \"Reiniciar ahora y comprobar\". El resultado aparece aquí en el próximo análisis.");
        }
    }
}
