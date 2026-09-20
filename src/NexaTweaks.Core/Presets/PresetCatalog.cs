using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Presets;

/// <summary>A named bundle of tweaks the user can apply in one go.</summary>
public sealed record Preset(string Id, string Name, string Description, IReadOnlyList<ITweak> Tweaks)
{
    public IReadOnlyDictionary<TweakCategory, int> CountsByCategory() =>
        Tweaks.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => g.Count());
}

/// <summary>
/// Three levels of the same idea: each one contains the previous plus a bit more.
/// Never included anywhere: riesgo alto, desinstalar apps, la categoría Avanzado, y los permisos
/// de cámara/micrófono/contactos - todos ésos se activan a mano y con confirmación.
/// </summary>
public static class PresetCatalog
{
    private static readonly string[] NeverInPresets =
    {
        "priv.camera", "priv.microphone", "priv.contacts", "priv.calendar",
        "priv.notifications", "priv.accountinfo",
    };

    private static IReadOnlyList<Preset>? _all;

    public static IReadOnlyList<Preset> All => _all ??= new List<Preset>
    {
        new("preset.seguro", "Seguro",
            "Lo que aplicaríamos en cualquier PC: telemetría, servicios que nadie usa y ajustes de respuesta del sistema. Nada llamativo, nada que echar de menos.",
            Eligible().Where(t => t.Risk == RiskLevel.Safe && t.DefaultEnabled).ToList()),

        new("preset.medio", "Medio",
            "Todo lo del nivel Seguro más el resto de ajustes sin riesgo: más privacidad, más interfaz y más red.",
            Eligible().Where(t => t.Risk == RiskLevel.Safe).ToList()),

        new("preset.extremo", "Extremo",
            "Todo lo del nivel Medio más los ajustes avanzados: tiempos del sistema, prioridad de la ventana activa y servicios que alguna función puntual puede necesitar.",
            Eligible().Where(t => t.Risk is RiskLevel.Safe or RiskLevel.Advanced).ToList()),
    };

    /// <summary>Tweaks a preset is allowed to touch at all.</summary>
    private static IEnumerable<ITweak> Eligible() => TweakCatalog.All
        .Where(t => t.Category is not (TweakCategory.Advanced or TweakCategory.Debloat))
        .Where(t => t.IsReversible)
        .Where(t => !NeverInPresets.Contains(t.Id));
}
