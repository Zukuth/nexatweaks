using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    // NOTE: this category is user-facing "power user" territory, not a place for tweaks that
    // disable Windows security controls (Defender, UAC, Core Isolation, Windows Update). See
    // NexaTweaks.Tests.SecurityCatalogTests for the regression test that guards this — a build
    // that shipped four such tweaks got flagged by Microsoft Defender as TrojanWin32/Bearfoos.A!ml.
    public static IReadOnlyList<ITweak> Advanced { get; } = new List<ITweak>
    {
        new ServiceStateTweak
        {
            Id = "adv.xbox.xblauthmanager",
            Name = "Desactivar servicios de Xbox",
            Description = "Deshabilita XblAuthManager (autenticación de Xbox Live), innecesario si no usas Xbox app/Game Pass.",
            Category = TweakCategory.Advanced,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "XblAuthManager",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "adv.programcompat",
            Name = "Desactivar Asistente de compatibilidad",
            Description = "Deshabilita Program Compatibility Assistant (PcaSvc). Puede impedir avisos útiles para software antiguo.",
            Category = TweakCategory.Advanced,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "PcaSvc",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "adv.xbox.gamesave",
            Name = "Desactivar guardado de partidas Xbox",
            Description = "Deshabilita XblGameSave. No lo uses si sincronizás partidas con Xbox/Game Pass.",
            Category = TweakCategory.Advanced,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "XblGameSave",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "adv.xbox.network",
            Name = "Desactivar red Xbox",
            Description = "Deshabilita XboxNetApiSvc. No lo uses si dependés de multijugador Xbox/Game Pass.",
            Category = TweakCategory.Advanced,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "XboxNetApiSvc",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "adv.xbox.gip",
            Name = "Desactivar accesorios Xbox",
            Description = "Deshabilita XboxGipSvc. Puede afectar mandos y accesorios Xbox.",
            Category = TweakCategory.Advanced,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "XboxGipSvc",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak { Id = "adv.nvidiatelemetry", Name = "Desactivar telemetría NVIDIA", Description = "Deshabilita NvTelemetryContainer si está instalado. Puede no existir en versiones modernas del driver.", Category = TweakCategory.Advanced, Risk = RiskLevel.Advanced, DefaultEnabled = false, ServiceName = "NvTelemetryContainer", DesiredStartMode = "Disabled" },
    };
}
