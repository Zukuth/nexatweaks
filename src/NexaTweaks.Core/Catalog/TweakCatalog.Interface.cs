using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    private const string ExplorerAdvanced = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    /// <summary>
    /// Taskbar and File Explorer preferences. All per-user (HKCU) and instant: most take effect
    /// after reiniciar el Explorador, que la app ofrece desde Reparación.
    /// </summary>
    public static IReadOnlyList<ITweak> Interface { get; } = new List<ITweak>
    {
        Ui("ui.taskbar.left", "Barra de tareas a la izquierda",
            "Alinea los iconos de la barra de tareas a la izquierda, como en Windows 10.",
            ExplorerAdvanced, "TaskbarAl", 0),

        Ui("ui.taskbar.search", "Quitar el cuadro de búsqueda",
            "Deja la barra de tareas sin la caja de búsqueda; el menú Inicio sigue buscando igual.",
            @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0),

        Ui("ui.taskbar.taskview", "Quitar el botón Vista de tareas",
            "Oculta el botón de escritorios virtuales. El atajo Win+Tab sigue funcionando.",
            ExplorerAdvanced, "ShowTaskViewButton", 0),

        Ui("ui.taskbar.chat", "Quitar el botón de Chat",
            "Oculta el icono de Microsoft Teams (Chat) de la barra de tareas.",
            ExplorerAdvanced, "TaskbarMn", 0),

        Ui("ui.explorer.extensions", "Mostrar extensiones de archivo",
            "Muestra la extensión real de cada archivo. Ayuda a detectar un .exe disfrazado de .pdf.",
            ExplorerAdvanced, "HideFileExt", 0, defaultEnabled: true),

        Ui("ui.explorer.hidden", "Mostrar archivos ocultos",
            "Muestra los archivos y carpetas marcados como ocultos.",
            ExplorerAdvanced, "Hidden", 1),

        Ui("ui.explorer.thispc", "Abrir el Explorador en Este equipo",
            "El Explorador abre en Este equipo en lugar de en Acceso rápido.",
            ExplorerAdvanced, "LaunchTo", 1, defaultEnabled: true),

        Ui("ui.menushowdelay", "Menús sin retardo",
            "Quita el retardo de 400 ms con el que se despliegan los menús.",
            @"Control Panel\Desktop", "MenuShowDelay", "0", RegistryValueKind.String),

        new RegistryTweak
        {
            Id = "ui.verbosestatus",
            Name = "Mensajes detallados al iniciar y apagar",
            Description = "Muestra qué está haciendo Windows al encender o apagar, en vez de un simple «Espera».",
            Category = TweakCategory.Interface,
            Risk = RiskLevel.Safe,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
            ValueName = "VerboseStatus",
            EnabledValue = 1,
        },
    };

    private static RegistryTweak Ui(string id, string name, string description, string subKey,
        string valueName, object enabledValue, RegistryValueKind kind = RegistryValueKind.DWord,
        bool defaultEnabled = false) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Category = TweakCategory.Interface,
        Risk = RiskLevel.Safe,
        DefaultEnabled = defaultEnabled,
        Hive = RegistryHive.CurrentUser,
        SubKey = subKey,
        ValueName = valueName,
        EnabledValue = enabledValue,
        ValueKind = kind,
    };
}
