using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    private const string AppPrivacyPolicy = @"SOFTWARE\Policies\Microsoft\Windows\AppPrivacy";
    private const string ContentDelivery = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string CloudContentPolicy = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent";

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
