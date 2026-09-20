using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    private const string AppPrivacyPolicy = @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy";
    private const string ContentDelivery = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string CloudContentPolicy = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent";
    private const string WindowsAiPolicy = @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI";
    private const string PaintPolicy = @"Software\Microsoft\Windows\CurrentVersion\Policies\Paint";

    /// <summary>
    /// Data collection and "helpful" cloud features of modern Windows. The app-permission blocks
    /// (camera, microphone, contacts...) use the AppPrivacy policy value 2 = Deny, which affects
    /// Store apps only - a desktop program like OBS or Zoom keeps working.
    /// </summary>
    public static IReadOnlyList<ITweak> Privacy { get; } = new List<ITweak>
    {
        Priv("priv.copilot", "Desactivar Windows Copilot",
            "Quita el asistente de IA de la barra de tareas y evita que se cargue con la sesión.",
            RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.recall", "Desactivar Windows Recall",
            "Impide que Windows guarde capturas periódicas de todo lo que haces en pantalla.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.widgets", "Desactivar Widgets",
            "Apaga el panel de noticias e intereses, que consume RAM y red en segundo plano.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.wifisense", "Desactivar Wi-Fi Sense",
            "Evita la conexión automática a redes abiertas sugeridas por Microsoft.",
            RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\WcmSvc\wifinetworkmanager\config", "AutoConnectAllowedOEM", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.inkcollection", "No recopilar escritura a mano",
            "Impide que Windows guarde muestras de lo que escribes con lápiz digital.",
            RegistryHive.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.inputpersonalization", "No recopilar patrones de tecleo",
            "Desactiva el aprendizaje de lo que escribes para personalizar sugerencias.",
            RegistryHive.CurrentUser, @"Software\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.siuf", "Desactivar encuestas de Windows",
            "Windows deja de preguntarte qué te parece tal función (SIUF).",
            RegistryHive.CurrentUser, @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.feedbackfrequency", "Nunca pedir comentarios",
            "Pone a cero la frecuencia con la que Windows solicita comentarios.",
            RegistryHive.CurrentUser, @"Software\Microsoft\Siuf\Rules", "PeriodInNanoSeconds", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.tailoredexperiences", "Desactivar experiencias personalizadas",
            "Microsoft deja de usar tus datos de diagnóstico para sugerirte contenido y anuncios.",
            RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy",
            "TailoredExperiencesWithDiagnosticDataEnabled", 0, RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.consumerfeatures", "Desactivar funciones de consumidor",
            "Impide que Windows instale por su cuenta apps promocionadas (juegos y pruebas gratis).",
            RegistryHive.LocalMachine, CloudContentPolicy, "DisableWindowsConsumerFeatures", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.cloudcontent", "Desactivar contenido de la nube",
            "Quita las sugerencias y el contenido patrocinado que Microsoft envía a Windows.",
            RegistryHive.LocalMachine, CloudContentPolicy, "DisableCloudOptimizedContent", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.suggestedapps", "Desactivar apps sugeridas",
            "Windows deja de instalar en silencio las aplicaciones que sugiere la Tienda.",
            RegistryHive.CurrentUser, ContentDelivery, "SilentInstalledAppsEnabled", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.subscribedcontent", "Desactivar contenido suscrito",
            "Quita las sugerencias de apps y el contenido patrocinado del menú Inicio.",
            RegistryHive.CurrentUser, ContentDelivery, "SubscribedContent-338388Enabled", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.oempreinstalled", "Desactivar apps preinstaladas del fabricante",
            "Impide que Windows reinstale las apps promocionales que trae el equipo de fábrica.",
            RegistryHive.CurrentUser, ContentDelivery, "OemPreInstalledAppsEnabled", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.clipboardhistory", "Desactivar historial del portapapeles",
            "Windows deja de guardar lo que copias (Win+V). Copiar y pegar normal sigue igual.",
            RegistryHive.CurrentUser, @"Software\Microsoft\Clipboard", "EnableClipboardHistory", 0,
            RiskLevel.Advanced),

        Priv("priv.speechcloud", "Desactivar reconocimiento de voz en la nube",
            "Tu voz deja de enviarse a Microsoft para reconocerla; el dictado local sigue disponible.",
            RegistryHive.CurrentUser, @"Software\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 0,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.appdiagnostics", "Bloquear diagnósticos entre apps",
            "Impide que unas aplicaciones consulten los datos de diagnóstico de otras.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsGetDiagnosticInfo", 2,
            RiskLevel.Safe, defaultEnabled: true),

        // Permisos de apps de la Tienda: 2 = Denegar. Son intrusivos, así que van marcados como
        // avanzados y desactivados por defecto.
        Priv("priv.camera", "Bloquear cámara para apps de la Tienda",
            "Deniega el acceso a la cámara a las aplicaciones de la Tienda. Los programas de escritorio (OBS, Zoom) no se ven afectados.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessCamera", 2, RiskLevel.Advanced),

        Priv("priv.microphone", "Bloquear micrófono para apps de la Tienda",
            "Deniega el acceso al micrófono a las aplicaciones de la Tienda. Los programas de escritorio no se ven afectados.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessMicrophone", 2, RiskLevel.Advanced),

        Priv("priv.contacts", "Bloquear contactos para apps de la Tienda",
            "Deniega el acceso a tu lista de contactos de Windows.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessContacts", 2, RiskLevel.Advanced),

        Priv("priv.calendar", "Bloquear calendario para apps de la Tienda",
            "Deniega el acceso a tu calendario y a tus eventos.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessCalendar", 2, RiskLevel.Advanced),

        Priv("priv.notifications", "Bloquear notificaciones para apps de la Tienda",
            "Impide que otras apps lean el contenido de tus notificaciones.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessNotifications", 2, RiskLevel.Advanced),

        Priv("priv.accountinfo", "Bloquear datos de cuenta para apps de la Tienda",
            "Deniega el acceso a tu nombre y foto de la cuenta de Windows.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessAccountInfo", 2, RiskLevel.Advanced),

        Priv("priv.messaging", "Bloquear mensajería para apps de la Tienda",
            "Impide que las apps lean tus SMS y mensajes de chat sincronizados.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessMessaging", 2, RiskLevel.Advanced),

        Priv("priv.email", "Bloquear correo para apps de la Tienda",
            "Impide que las apps lean tu correo y sus adjuntos.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessEmail", 2, RiskLevel.Advanced),

        Priv("priv.callhistory", "Bloquear historial de llamadas",
            "Impide que las apps consulten las llamadas sincronizadas desde el móvil.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessCallHistory", 2, RiskLevel.Advanced),

        Priv("priv.phone", "Bloquear funciones de teléfono",
            "Impide que las apps usen las funciones de llamada del equipo.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessPhone", 2, RiskLevel.Advanced),

        Priv("priv.tasks", "Bloquear tareas y recordatorios",
            "Impide que las apps lean tus tareas y recordatorios.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessTasks", 2, RiskLevel.Advanced),

        Priv("priv.motion", "Bloquear sensores de movimiento",
            "Impide que las apps lean el acelerómetro y los sensores de orientación.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessMotion", 2, RiskLevel.Advanced),

        Priv("priv.radios", "Bloquear control de radios",
            "Impide que las apps enciendan o apaguen el Wi-Fi y el Bluetooth por su cuenta.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessRadios", 2, RiskLevel.Advanced),

        Priv("priv.location.apps", "Bloquear ubicación para apps de la Tienda",
            "Deniega el acceso a tu ubicación a las aplicaciones de la Tienda.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessLocation", 2, RiskLevel.Advanced),

        Priv("priv.trusteddevices", "Bloquear dispositivos de confianza",
            "Impide que las apps se comuniquen con dispositivos emparejados sin permiso.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessTrustedDevices", 2, RiskLevel.Advanced),

        Priv("priv.devicediscovery", "Bloquear sincronización con dispositivos",
            "Impide que las apps descubran y se sincronicen con dispositivos de la red sin avisarte.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsSyncWithDevices", 2, RiskLevel.Advanced),

        Priv("priv.filesystem", "Bloquear acceso al sistema de archivos",
            "Deniega a las apps de la Tienda el acceso general a tus bibliotecas y unidades.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessFileSystem", 2, RiskLevel.Advanced),

        Priv("priv.voiceactivation", "Bloquear activación por voz",
            "Impide que las apps queden escuchando para activarse con una palabra clave.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsActivateWithVoice", 2,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.capture.border", "Exigir aviso al capturar la pantalla",
            "Las apps no pueden grabar tu pantalla sin el borde amarillo que avisa de la grabación.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessGraphicsCaptureWithoutBorder", 2,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.capture.programmatic", "Bloquear capturas automáticas",
            "Impide que las apps capturen o graben la pantalla sin que tú lo pidas.",
            RegistryHive.LocalMachine, AppPrivacyPolicy, "LetAppsAccessGraphicsCaptureProgrammatic", 2,
            RiskLevel.Advanced),

        // Funciones de IA de Windows. Claves confirmadas en la documentación de Microsoft
        // (Policy CSP - WindowsAI y «Manage AI features in Notepad»).
        Priv("priv.ai.clicktodo", "Desactivar Click to Do",
            "Quita la función que captura tu pantalla para ofrecerte acciones sobre lo que hay en ella.",
            RegistryHive.LocalMachine, WindowsAiPolicy, "DisableClickToDo", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.ai.notepad", "Desactivar la IA del Bloc de notas",
            "Quita las funciones de reescritura y resumen con IA del Bloc de notas.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\WindowsNotepad", "DisableAIFeatures", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.ai.paint.cocreator", "Desactivar Cocreator de Paint",
            "Quita el generador de imágenes con IA de Paint.",
            RegistryHive.LocalMachine, PaintPolicy, "DisableCocreator", 1,
            RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.ai.paint.fill", "Desactivar relleno generativo de Paint",
            "Quita el relleno de imágenes con IA de Paint.",
            RegistryHive.LocalMachine, PaintPolicy, "DisableGenerativeFill", 1,
            RiskLevel.Safe, defaultEnabled: true),

        // Telemetría de otros productos y sincronización con la nube.
        Priv("priv.office.telemetry", "Desactivar telemetría de Office",
            "Office deja de enviar datos de uso a Microsoft (SendTelemetry = deshabilitado).",
            RegistryHive.CurrentUser, @"Software\Policies\Microsoft\office\16.0\common\clienttelemetry",
            "SendTelemetry", 3, RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.mediaplayer.metadata", "Desactivar metadatos de Windows Media Player",
            "Deja de consultar carátulas e información de tus discos y archivos en internet.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\WindowsMediaPlayer",
            "PreventCDDVDMetadataRetrieval", 1, RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.search.cloud", "Desactivar búsqueda en la nube",
            "Windows Search deja de enviar a Microsoft lo que buscas en tu equipo.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
            "AllowCloudSearch", 0, RiskLevel.Safe, defaultEnabled: true),

        Priv("priv.findmydevice", "Desactivar Encontrar mi dispositivo",
            "Microsoft deja de poder localizar este equipo a distancia.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\FindMyDevice", "AllowFindMyDevice", 0,
            RiskLevel.Advanced),

        Priv("priv.cloudsync", "Desactivar sincronización de ajustes",
            "Deja de sincronizar temas, contraseñas y preferencias entre tus equipos.",
            RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\SettingSync",
            "DisableSettingSync", 2, RiskLevel.Advanced),
    };

    private static RegistryTweak Priv(string id, string name, string description, RegistryHive hive,
        string subKey, string valueName, int enabledValue, RiskLevel risk, bool defaultEnabled = false) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Category = TweakCategory.Privacy,
        Risk = risk,
        DefaultEnabled = defaultEnabled,
        Hive = hive,
        SubKey = subKey,
        ValueName = valueName,
        EnabledValue = enabledValue,
    };
}
