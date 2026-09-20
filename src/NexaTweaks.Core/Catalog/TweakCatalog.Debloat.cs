using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    /// <summary>
    /// Preinstalled Store apps that most people never open. Desinstalar es de ida: si te
    /// arrepientes, se reinstalan desde Microsoft Store. Por eso ninguna viene marcada.
    /// Nunca se listan la Tienda, la interfaz de Defender, winget, el Terminal, las librerías de
    /// las que dependen otras apps ni Gaming Services (Game Pass deja de funcionar sin ella).
    /// </summary>
    public static IReadOnlyList<ITweak> Debloat { get; } = new List<ITweak>
    {
        App("app.3dbuilder", "Microsoft.3DBuilder", "3D Builder",
            "Editor de modelos 3D que Microsoft ya no mantiene."),

        App("app.mixedreality", "Microsoft.MixedReality.Portal", "Portal de Realidad Mixta",
            "Portal para visores de realidad mixta, discontinuado en Windows 11."),

        App("app.solitaire", "Microsoft.MicrosoftSolitaireCollection", "Solitario",
            "Colección de solitarios con anuncios y suscripción."),

        App("app.gethelp", "Microsoft.GetHelp", "Obtener ayuda",
            "Asistente de soporte de Microsoft. La ayuda sigue disponible en su web."),

        App("app.tips", "Microsoft.Getstarted", "Sugerencias",
            "App de consejos y primeros pasos de Windows."),

        App("app.feedbackhub", "Microsoft.WindowsFeedbackHub", "Centro de opiniones",
            "Envía comentarios y datos de uso a Microsoft."),

        App("app.maps", "Microsoft.WindowsMaps", "Mapas",
            "Mapas sin conexión de Windows. Descarga datos en segundo plano."),

        App("app.news", "Microsoft.BingNews", "Noticias",
            "Noticias de Bing, la fuente del panel de Widgets."),

        App("app.weather", "Microsoft.BingWeather", "El Tiempo",
            "App del tiempo de Bing."),

        App("app.people", "Microsoft.People", "Contactos",
            "Agenda de contactos de Windows, usada por Correo y Calendario."),

        App("app.skype", "Microsoft.SkypeApp", "Skype",
            "Versión preinstalada de Skype."),

        App("app.clipchamp", "Clipchamp.Clipchamp", "Clipchamp",
            "Editor de vídeo que Windows 11 preinstala."),

        App("app.paint3d", "Microsoft.MSPaint", "Paint 3D",
            "Editor 3D discontinuado. No es el Paint clásico, que se conserva."),

        App("app.soundrecorder", "Microsoft.WindowsSoundRecorder", "Grabadora de voz",
            "Grabadora de sonido de Windows."),

        App("app.yourphone", "Microsoft.YourPhone", "Vincular al teléfono",
            "Sincroniza mensajes y fotos con el móvil. Corre en segundo plano."),

        App("app.alarms", "Microsoft.WindowsAlarms", "Alarmas y reloj",
            "Alarmas, temporizador y cronómetro."),

        App("app.camera", "Microsoft.WindowsCamera", "Cámara",
            "App de cámara de Windows. Solo si no usas la webcam desde ella."),

        App("app.mailcalendar", "microsoft.windowscommunicationsapps", "Correo y Calendario",
            "Cliente de correo y calendario clásico de Windows, ya reemplazado por Outlook."),

        App("app.onenote", "Microsoft.Office.OneNote", "OneNote para Windows 10",
            "Versión UWP de OneNote, sustituida por la de Office."),

        App("app.todos", "Microsoft.Todos", "Microsoft To Do",
            "App de listas de tareas."),

        App("app.familysafety", "MicrosoftCorporationII.MicrosoftFamily", "Microsoft Family",
            "Control parental y límites de uso en familia."),

        App("app.quickassist", "MicrosoftCorporationII.QuickAssist", "Asistencia rápida",
            "Permite que alguien controle tu PC de forma remota para ayudarte."),

        App("app.xboxapp", "Microsoft.XboxApp", "Xbox (app antigua)",
            "App de Xbox heredada. No afecta a Game Bar ni a Game Pass."),

        App("app.xboxtcui", "Microsoft.Xbox.TCUI", "Xbox TCUI",
            "Interfaz común de Xbox para invitaciones y perfiles dentro de juegos."),

        App("app.xboxgameoverlay", "Microsoft.XboxGameOverlay", "Superposición de Xbox",
            "Capa de Game Bar que se dibuja sobre los juegos."),

        App("app.xboxspeech", "Microsoft.XboxSpeechToTextOverlay", "Subtítulos de Xbox",
            "Superposición de voz a texto para juegos de Xbox."),

        App("app.zune", "Microsoft.ZuneVideo", "Películas y TV",
            "Reproductor y tienda de vídeo de Microsoft."),

        App("app.zunemusic", "Microsoft.ZuneMusic", "Groove Música",
            "Reproductor de música heredado."),

        App("app.cortana", "Microsoft.549981C3F5F10", "Cortana",
            "El asistente de voz de Windows, ya retirado de la barra de tareas."),

        App("app.bingfinance", "Microsoft.BingFinance", "Finanzas",
            "App de bolsa y finanzas de Bing."),

        App("app.bingsports", "Microsoft.BingSports", "Deportes",
            "App de resultados deportivos de Bing."),

        App("app.bingtranslator", "Microsoft.BingTranslator", "Traductor",
            "Traductor de Microsoft en versión de la Tienda."),

        App("app.officehub", "Microsoft.MicrosoftOfficeHub", "Office (acceso)",
            "Acceso directo a Office online. No es Office instalado: no se desinstala nada real."),

        App("app.teams", "MSTeams", "Microsoft Teams",
            "Teams personal preinstalado en Windows 11."),

        App("app.devhome", "Microsoft.Windows.DevHome", "Dev Home",
            "Panel para desarrolladores que Windows 11 instala de fábrica."),

        App("app.outlooknew", "Microsoft.OutlookForWindows", "Outlook (nuevo)",
            "El nuevo Outlook basado en web que Microsoft preinstala."),

        App("app.copilot", "Microsoft.Copilot", "Copilot",
            "La app del asistente de IA. Complementa al ajuste que lo quita de la barra."),

        App("app.journal", "Microsoft.MicrosoftJournal", "Journal",
            "App de notas a mano para pantallas táctiles."),

        App("app.whiteboard", "Microsoft.Whiteboard", "Whiteboard",
            "Pizarra colaborativa de Microsoft."),

        App("app.powerautomate", "Microsoft.PowerAutomateDesktop", "Power Automate",
            "Automatización de tareas de escritorio, preinstalada en Windows 11."),

        App("app.print3d", "Microsoft.Print3D", "Print 3D",
            "App de impresión 3D discontinuada."),

        App("app.speedtest", "Microsoft.NetworkSpeedTest", "Network Speed Test",
            "Medidor de velocidad de red de Microsoft."),

        App("app.remotedesktop", "Microsoft.RemoteDesktop", "Escritorio remoto (Tienda)",
            "Cliente de escritorio remoto de la Tienda. El de Windows (mstsc) no se toca."),

        App("app.readinglist", "Microsoft.WindowsReadingList", "Lista de lectura",
            "App de lista de lectura heredada."),

        App("app.messaging", "Microsoft.Messaging", "Mensajes",
            "App de SMS heredada de Windows."),

        App("app.oneconnect", "Microsoft.OneConnect", "Wi-Fi de pago",
            "App para planes de datos móviles y Wi-Fi de pago."),

        App("app.wallet", "Microsoft.Wallet", "Cartera",
            "Cartera de pagos de Microsoft, descontinuada."),

        App("app.powerbi", "Microsoft.MicrosoftPowerBIForWindows", "Power BI",
            "Visor de informes de Power BI."),

        // OneDrive no es una app de la Tienda: se quita con su propio desinstalador.
        new ShellCommandTweak
        {
            Id = "app.onedrive",
            Name = "Desinstalar OneDrive",
            Description = "Quita OneDrive del equipo. Tus archivos ya sincronizados se quedan en la carpeta local; los de la nube siguen en onedrive.com.",
            Category = TweakCategory.Debloat,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            IsReversible = false,
            CaptureState = () => OneDriveSetup() is null ? "ausente" : "instalado",
            ApplyCommand = () => $"taskkill /f /im OneDrive.exe & \"{OneDriveSetup()}\" /uninstall",
            RevertCommand = _ => "cmd /c exit 0",
            AppliedCheck = () => OneDriveSetup() is null,
        },
    };

    /// <summary>Ruta del desinstalador de OneDrive, que cambia entre equipos de 64 y 32 bits.</summary>
    private static string? OneDriveSetup()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64", "OneDriveSetup.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "OneDriveSetup.exe"),
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static AppxPackageTweak App(string id, string packageName, string name, string description) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Category = TweakCategory.Debloat,
        Risk = RiskLevel.Advanced,
        DefaultEnabled = false,
        PackageName = packageName,
    };
}
