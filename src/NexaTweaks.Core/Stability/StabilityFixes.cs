using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Stability;

/// <summary>
/// Registry values the stability checks can put back to Windows' defaults. They are regular
/// <see cref="RegistryTweak"/>s so the engine snapshots the old value and the Backup section can
/// undo them; that section needs <see cref="All"/> to recognise their ids.
/// </summary>
public static class StabilityFixes
{
    internal const string GraphicsDriversKey = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
    internal const string WindowsUpdatePoliciesKey = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
    internal const string PowerThrottlingKey = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling";

    public static RegistryTweak TdrDelay { get; } = Fix("stability.tdrdelay", "Restaurar TdrDelay",
        "Vuelve a dar 2 segundos a la GPU antes de reiniciar su driver (valor de Windows).",
        TweakCategory.Gpu, GraphicsDriversKey, "TdrDelay", 2, restart: true);

    public static RegistryTweak TdrDdiDelay { get; } = Fix("stability.tdrddidelay", "Restaurar TdrDdiDelay",
        "Vuelve a dar 5 segundos a los hilos del driver de vídeo (valor de Windows).",
        TweakCategory.Gpu, GraphicsDriversKey, "TdrDdiDelay", 5, restart: true);

    public static RegistryTweak UpdateSafeguards { get; } = Fix("stability.wu.safeguards",
        "Reactivar protecciones de Windows Update",
        "Permite que Windows bloquee actualizaciones con incompatibilidades conocidas para este equipo.",
        TweakCategory.Windows, WindowsUpdatePoliciesKey, "DisableWUfBSafeguards", 0, restart: false);

    public static RegistryTweak UpdateDrivers { get; } = Fix("stability.wu.drivers",
        "Permitir drivers por Windows Update",
        "Deja que Windows Update instale drivers actualizados junto con las actualizaciones de calidad.",
        TweakCategory.Windows, WindowsUpdatePoliciesKey, "ExcludeWUDriversInQualityUpdate", 0, restart: false);

    public static RegistryTweak PowerThrottling { get; } = Fix("stability.powerthrottling",
        "Reactivar Power Throttling",
        "Deja que Windows limite los procesos en segundo plano para reducir calor y consumo.",
        TweakCategory.Windows, PowerThrottlingKey, "PowerThrottlingOff", 0, restart: true);

    public static IReadOnlyList<ITweak> All { get; } =
        new ITweak[] { TdrDelay, TdrDdiDelay, UpdateSafeguards, UpdateDrivers, PowerThrottling };

    private static RegistryTweak Fix(string id, string name, string description, TweakCategory category,
        string subKey, string valueName, int value, bool restart) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Category = category,
        Risk = RiskLevel.Safe,
        RequiresRestart = restart,
        Hive = RegistryHive.LocalMachine,
        SubKey = subKey,
        ValueName = valueName,
        EnabledValue = value,
    };
}
