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

        Ui("ui.snapflyout", "Quitar sugerencias al acoplar ventanas",
            "Al arrastrar una ventana a un borde, Windows deja de proponerte con qué emparejarla.",
            ExplorerAdvanced, "EnableSnapAssistFlyout", 0),

        Ui("ui.startmenurecs", "Quitar recomendados del menú Inicio",
            "Deja de sugerirte archivos y apps recién usados en el menú Inicio.",
            ExplorerAdvanced, "Start_IrisRecommendations", 0),

        Ui("ui.iconsonly", "Iconos en vez de miniaturas",
            "Muestra el icono del tipo de archivo en lugar de generar una vista previa. Abre antes las carpetas con muchas fotos o vídeos.",
            ExplorerAdvanced, "IconsOnly", 1),

        Ui("ui.endtask", "«Finalizar tarea» en la barra de tareas",
            "Añade la opción de cerrar a la fuerza un programa colgado con el botón derecho en su icono de la barra.",
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings",
            "TaskbarEndTask", 1, defaultEnabled: true),

        Ui("ui.darkmode.apps", "Tema oscuro en las aplicaciones",
            "Pone en oscuro las apps que respetan el tema de Windows.",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0),

        Ui("ui.darkmode.system", "Tema oscuro en Windows",
            "Pone en oscuro la barra de tareas, el menú Inicio y el centro de notificaciones.",
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0),

        Ui("ui.spotlight", "Quitar Windows Spotlight de la pantalla de bloqueo",
            "Deja de descargar imágenes y anuncios de Microsoft para la pantalla de bloqueo.",
            @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 0),

        Ui("ui.numlock", "NumLock activado al iniciar",
            "Enciende el teclado numérico al arrancar Windows, antes de escribir la contraseña.",
            @"Control Panel\Keyboard", "InitialKeyboardIndicators", "2", RegistryValueKind.String),

        Ui("ui.stickykeys", "Desactivar teclas especiales",
            "Quita el aviso que salta al pulsar Mayús cinco veces, molesto sobre todo jugando.",
            @"Control Panel\Accessibility\StickyKeys", "Flags", "506", RegistryValueKind.String),

        // El menú de Windows 11 se apaga vaciando el valor predeterminado de este CLSID, que es
        // justo el que lo activa. Al revertir, el valor se borra y vuelve el menú nuevo.
        Ui("ui.classicmenu", "Menú contextual clásico",
            "Devuelve el menú del botón derecho completo de Windows 10, sin el paso «Mostrar más opciones». Requiere reiniciar el Explorador.",
            @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "",
            RegistryValueKind.String),

        new RegistryTweak
        {
            Id = "ui.nolockscreen",
            Name = "Saltar la pantalla de bloqueo",
            Description = "Va directo a la pantalla de contraseña al encender o desbloquear el equipo.",
            Category = TweakCategory.Interface,
            Risk = RiskLevel.Safe,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\Personalization",
            ValueName = "NoLockScreen",
            EnabledValue = 1,
        },
        new RegistryTweak
        {
            Id = "ui.loginblur",
            Name = "Quitar el desenfoque del inicio de sesión",
            Description = "Muestra el fondo nítido en la pantalla de inicio de sesión, sin el efecto de cristal.",
            Category = TweakCategory.Interface,
            Risk = RiskLevel.Safe,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\System",
            ValueName = "DisableAcrylicBackgroundOnLogon",
            EnabledValue = 1,
        },
        new RegistryTweak
        {
            Id = "ui.detailedbsod",
            Name = "Pantallazo azul con detalles",
            Description = "Muestra los parámetros del error en la pantalla azul, útiles para diagnosticar qué falló.",
            Category = TweakCategory.Interface,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control\CrashControl",
            ValueName = "DisplayParameters",
            EnabledValue = 1,
        },
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
