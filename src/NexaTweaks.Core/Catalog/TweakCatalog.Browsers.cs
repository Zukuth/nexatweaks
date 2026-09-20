using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    private const string EdgePolicy = @"SOFTWARE\Policies\Microsoft\Edge";
    private const string ChromePolicy = @"SOFTWARE\Policies\Google\Chrome";
    private const string BravePolicy = @"SOFTWARE\Policies\BraveSoftware\Brave";

    /// <summary>
    /// Políticas oficiales de cada navegador (las mismas que usan las empresas), así que no hay
    /// parches ni archivos modificados: se escriben en su clave de directivas y se quitan igual.
    /// Nunca se tocan las protecciones: navegación segura, SmartScreen, avisos de certificado ni
    /// el gestor de contraseñas.
    /// Si el navegador no está instalado, la política queda escrita y sin efecto.
    /// </summary>
    public static IReadOnlyList<ITweak> Browsers { get; } = new List<ITweak>
    {
        // --- Microsoft Edge ---
        Browser("browser.edge.telemetry", "Edge: desactivar telemetría",
            "Edge deja de enviar métricas de uso a Microsoft.",
            EdgePolicy, "MetricsReportingEnabled", 0),

        Browser("browser.edge.sync", "Edge: desactivar sincronización",
            "No sincroniza favoritos, contraseñas ni historial con tu cuenta Microsoft.",
            EdgePolicy, "SyncDisabled", 1),

        Browser("browser.edge.personalization", "Edge: sin publicidad personalizada",
            "Deja de usar tu navegación para personalizar anuncios y contenido.",
            EdgePolicy, "PersonalizationReportingEnabled", 0),

        Browser("browser.edge.feedback", "Edge: quitar envío de comentarios",
            "Oculta el botón de comentarios y deja de enviar informes de experiencia.",
            EdgePolicy, "UserFeedbackAllowed", 0),

        Browser("browser.edge.shopping", "Edge: quitar asistente de compras",
            "Apaga el comparador de precios y los cupones que aparecen al navegar por tiendas.",
            EdgePolicy, "EdgeShoppingAssistantEnabled", 0),

        Browser("browser.edge.recommendations", "Edge: quitar recomendaciones",
            "Deja de sugerirte contenido y funciones dentro del navegador.",
            EdgePolicy, "ShowRecommendationsEnabled", 0),

        Browser("browser.edge.startupboost", "Edge: desactivar inicio rápido",
            "Edge deja de quedarse cargado en segundo plano al encender el equipo.",
            EdgePolicy, "StartupBoostEnabled", 0),

        Browser("browser.edge.background", "Edge: no seguir en segundo plano",
            "Al cerrar la ventana, Edge se cierra del todo en vez de seguir en la bandeja.",
            EdgePolicy, "BackgroundModeEnabled", 0),

        // --- Google Chrome ---
        Browser("browser.chrome.telemetry", "Chrome: desactivar telemetría",
            "Chrome deja de enviar estadísticas de uso e informes de fallos a Google.",
            ChromePolicy, "MetricsReportingEnabled", 0),

        Browser("browser.chrome.sync", "Chrome: desactivar sincronización",
            "No sincroniza marcadores, contraseñas ni historial con tu cuenta de Google.",
            ChromePolicy, "SyncDisabled", 1),

        Browser("browser.chrome.background", "Chrome: no seguir en segundo plano",
            "Al cerrar la ventana, Chrome deja de quedarse corriendo en la bandeja.",
            ChromePolicy, "BackgroundModeEnabled", 0),

        Browser("browser.chrome.urlcollection", "Chrome: no enviar las webs visitadas",
            "Desactiva la recopilación anónima de URLs que Google usa para sus servicios.",
            ChromePolicy, "UrlKeyedAnonymizedDataCollectionEnabled", 0),

        Browser("browser.chrome.searchsuggest", "Chrome: sin sugerencias de búsqueda",
            "Deja de enviar lo que escribes en la barra de direcciones al buscador.",
            ChromePolicy, "SearchSuggestEnabled", 0),

        // --- Brave ---
        Browser("browser.brave.telemetry", "Brave: desactivar telemetría",
            "Brave deja de enviar estadísticas de uso.",
            BravePolicy, "MetricsReportingEnabled", 0),

        Browser("browser.brave.sync", "Brave: desactivar sincronización",
            "Apaga la sincronización entre dispositivos de Brave.",
            BravePolicy, "SyncDisabled", 1),

        Browser("browser.brave.background", "Brave: no seguir en segundo plano",
            "Al cerrar la ventana, Brave se cierra del todo.",
            BravePolicy, "BackgroundModeEnabled", 0),

        Browser("browser.brave.rewards", "Brave: quitar Rewards",
            "Oculta el sistema de recompensas y anuncios de Brave.",
            BravePolicy, "BraveRewardsDisabled", 1),

        Browser("browser.brave.wallet", "Brave: quitar la cartera",
            "Oculta la cartera de criptomonedas integrada.",
            BravePolicy, "BraveWalletDisabled", 1),

        Browser("browser.brave.vpn", "Brave: quitar el VPN",
            "Oculta la promoción del VPN de pago de Brave.",
            BravePolicy, "BraveVPNDisabled", 1),

        Browser("browser.brave.ai", "Brave: quitar el asistente Leo",
            "Apaga el chat con IA integrado en Brave.",
            BravePolicy, "BraveAIChatEnabled", 0),
    };

    private static RegistryTweak Browser(string id, string name, string description, string subKey,
        string valueName, int enabledValue) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Category = TweakCategory.Browsers,
        Risk = RiskLevel.Safe,
        DefaultEnabled = false,
        Hive = RegistryHive.LocalMachine,
        SubKey = subKey,
        ValueName = valueName,
        EnabledValue = enabledValue,
    };
}
