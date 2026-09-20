using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Stability;

internal static class RegistryReads
{
    public static long? AsNumber(object? value) => value switch
    {
        int i => i,
        long l => l,
        uint u => u,
        string s when long.TryParse(s, out var parsed) => parsed,
        _ => null,
    };

    public static long? Hklm(IStabilityProbe probe, string subKey, string name) =>
        AsNumber(probe.GetRegistryValue(RegistryHive.LocalMachine, subKey, name));
}

/// <summary>TdrDelay/TdrDdiDelay raised far above the defaults: a hung GPU leaves the screen
/// black for that long instead of recovering in seconds.</summary>
public sealed class TdrDelayCheck : IStabilityCheck
{
    private const long MaxReasonableSeconds = 10;

    public string Id => "tdr";
    public string Name => "Recuperación de la GPU (TDR)";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        foreach (var fix in new[] { StabilityFixes.TdrDelay, StabilityFixes.TdrDdiDelay })
        {
            var seconds = RegistryReads.Hklm(probe, fix.SubKey, fix.ValueName);
            if (seconds is null || seconds <= MaxReasonableSeconds) continue;

            yield return new StabilityFinding(Id,
                $"{fix.ValueName} está en {seconds} s: si la GPU se cuelga, la pantalla queda en negro todo ese tiempo",
                StabilitySeverity.High,
                new[] { $@"HKLM\{fix.SubKey}\{fix.ValueName} = {seconds} (Windows usa {fix.EnabledValue})" },
                $"Restaura el valor de Windows ({fix.EnabledValue} s) para que el driver de vídeo se recupere solo. Requiere reiniciar.",
                fix);
        }
    }
}

/// <summary>Policies (often left by "optimized" Windows builds) that stop Windows Update from
/// holding back incompatible updates or from delivering drivers.</summary>
public sealed class WindowsUpdatePolicyCheck : IStabilityCheck
{
    public string Id => "wu-policies";
    public string Name => "Políticas de Windows Update";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        if (IsSet(probe, StabilityFixes.UpdateSafeguards))
        {
            yield return Finding(StabilityFixes.UpdateSafeguards,
                "Windows Update tiene desactivadas las protecciones de compatibilidad",
                "Sin estas protecciones Windows puede instalar actualizaciones que Microsoft sabe que fallan en este equipo.");
        }

        if (IsSet(probe, StabilityFixes.UpdateDrivers))
        {
            yield return Finding(StabilityFixes.UpdateDrivers,
                "Windows Update tiene bloqueada la instalación de drivers",
                "Los drivers antiguos son una causa habitual de pantallazos azules; deja que Windows Update los actualice.");
        }
    }

    private static bool IsSet(IStabilityProbe probe, RegistryTweak fix) =>
        RegistryReads.Hklm(probe, fix.SubKey, fix.ValueName) == 1;

    private StabilityFinding Finding(RegistryTweak fix, string title, string recommendation) =>
        new(Id, title, StabilitySeverity.Medium,
            new[] { $@"HKLM\{fix.SubKey}\{fix.ValueName} = 1" },
            recommendation, fix);
}

/// <summary>Power throttling disabled on a battery-powered machine, where it only adds heat.</summary>
public sealed class LaptopThrottlingCheck : IStabilityCheck
{
    public string Id => "laptop-throttling";
    public string Name => "Power Throttling en portátil";

    public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now)
    {
        var fix = StabilityFixes.PowerThrottling;
        if (RegistryReads.Hklm(probe, fix.SubKey, fix.ValueName) != 1) yield break;
        if (probe.QueryWmi("Win32_Battery", null, "Name").Count == 0) yield break;

        yield return new StabilityFinding(Id,
            "Power Throttling está desactivado en un portátil",
            StabilitySeverity.Low,
            new[] { $@"HKLM\{fix.SubKey}\{fix.ValueName} = 1", "Se detectó una batería (Win32_Battery)" },
            "En un portátil esto sube la temperatura, y el calor también causa cuelgues. Reactívalo salvo que lo necesites para jugar enchufado.",
            fix);
    }
}
