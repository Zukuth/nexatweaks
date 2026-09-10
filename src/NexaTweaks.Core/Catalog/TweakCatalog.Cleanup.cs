using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    public static IReadOnlyList<ITweak> Cleanup { get; } = new List<ITweak>
    {
        new FileCleanupTweak
        {
            Id = "clean.temp",
            Name = "Vaciar carpetas temporales",
            Description = "Borra el contenido de %TEMP% y C:\\Windows\\Temp. Libera espacio y elimina archivos residuales de instaladores/apps. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            TargetDirectories = new[]
            {
                Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
            },
        },
        new FileCleanupTweak
        {
            Id = "clean.prefetch",
            Name = "Limpiar caché de Prefetch",
            Description = "Borra C:\\Windows\\Prefetch. Windows la reconstruye automáticamente; útil si está muy fragmentada. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
            },
        },
        new WorkingSetTrimTweak
        {
            Id = "clean.ramtrim",
            Name = "Vaciar RAM en espera (working set trim)",
            Description = "Fuerza a todos los procesos accesibles a soltar páginas de memoria inactivas de vuelta a la lista de espera. Efecto real y medible, sin riesgo: Windows repagina lo que necesite.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
        },
        new FileCleanupTweak
        {
            Id = "clean.browser.chrome",
            Name = "Vaciar caché de Google Chrome",
            Description = "Borra la caché de Chrome (no tus contraseñas, historial ni marcadores). Chrome la reconstruye sola. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.BrowserCachePaths.Chrome(),
        },
        new FileCleanupTweak
        {
            Id = "clean.browser.edge",
            Name = "Vaciar caché de Microsoft Edge",
            Description = "Borra la caché de Edge (no tus contraseñas, historial ni marcadores). Edge la reconstruye sola. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.BrowserCachePaths.Edge(),
        },
        new FileCleanupTweak
        {
            Id = "clean.browser.firefox",
            Name = "Vaciar caché de Firefox",
            Description = "Borra la caché de Firefox (no tus contraseñas, historial ni marcadores). Firefox la reconstruye sola. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.BrowserCachePaths.Firefox(),
        },
        new FileCleanupTweak
        {
            Id = "clean.shadercache",
            Name = "Limpiar cachés de shaders",
            Description = "Borra cachés recreables de DirectX, NVIDIA y AMD. Puede haber una recompilación breve de shaders la próxima vez que abras un juego. Cierra los juegos y paneles GPU antes de usarlo. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.CleanupPathCatalog.ShaderCaches(),
        },
        new FileCleanupTweak
        {
            Id = "clean.discordcache",
            Name = "Vaciar caché de Discord",
            Description = "Borra únicamente cachés y reportes de fallos de Discord; no elimina tu cuenta, servidores ni mensajes. Cierra Discord primero. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.CleanupPathCatalog.DiscordCaches(),
        },
        new FileCleanupTweak
        {
            Id = "clean.launchercache",
            Name = "Vaciar caché de Steam y Epic",
            Description = "Borra cachés de interfaz de Steam y Epic Games Launcher, no juegos instalados ni partidas guardadas. Cierra los launchers primero. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.CleanupPathCatalog.LauncherCaches(),
        },
        new FileCleanupTweak
        {
            Id = "clean.crashreports",
            Name = "Limpiar reportes de fallos antiguos",
            Description = "Elimina volcados y reportes de errores locales de Windows. No afecta programas instalados; conserva reportes recientes para diagnóstico. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.CleanupPathCatalog.CrashReports(),
        },
        new FileCleanupTweak
        {
            Id = "clean.developercache",
            Name = "Vaciar cachés de desarrollo",
            Description = "Borra cachés recreables de npm, Yarn y NuGet, sin tocar proyectos ni paquetes instalados. La próxima restauración/instalación puede tardar más. No reversible.",
            Category = TweakCategory.Cleanup,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            TargetDirectories = NexaTweaks.Core.Cleanup.CleanupPathCatalog.DeveloperCaches(),
        },
    };
}
