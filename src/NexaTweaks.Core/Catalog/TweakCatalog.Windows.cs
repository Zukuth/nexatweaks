using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    public static IReadOnlyList<ITweak> Windows { get; } = new List<ITweak>
    {
        new RegistryTweak
        {
            Id = "win.telemetry",
            Name = "Reducir telemetría de Windows",
            Description = "Limita el nivel de diagnóstico y datos enviados a Microsoft (AllowTelemetry = Basic).",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection",
            ValueName = "AllowTelemetry",
            EnabledValue = 0,
        },
        new ServiceStateTweak
        {
            Id = "win.diagtrack",
            Name = "Deshabilitar Connected User Experiences and Telemetry",
            Description = "Detiene y deshabilita el servicio DiagTrack, responsable de recopilar y enviar datos de uso.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            ServiceName = "DiagTrack",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "win.dmwappush",
            Name = "Deshabilitar dmwappushservice",
            Description = "Servicio de enrutamiento de mensajes WAP push, sin uso en la mayoría de PCs de escritorio/gaming.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            ServiceName = "dmwappushservice",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "win.sysmain",
            Name = "SysMain (Superfetch) en modo manual",
            Description = "Reduce el precargado de apps en RAM/disco en segundo plano; recomendable en SSD para juegos.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            ServiceName = "SysMain",
            DesiredStartMode = "Manual",
        },
        new RegistryTweak
        {
            Id = "win.visualfx",
            Name = "Efectos visuales: Mejor rendimiento",
            Description = "Ajusta Windows para priorizar rendimiento sobre apariencia (equivalente a Panel de control > Rendimiento).",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
            ValueName = "VisualFXSetting",
            EnabledValue = 2,
        },
        new ShellCommandTweak
        {
            Id = "win.powerplan",
            Name = "Plan de energía: Alto rendimiento",
            Description = "Activa el plan de energía 'Alto rendimiento' de Windows (evita que el CPU/GPU bajen de frecuencia).",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            CaptureState = () => ExtractGuid(ShellCommandTweak.ExecCapture("powercfg /getactivescheme")),
            ApplyCommand = () => "powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
            RevertCommand = prior => $"powercfg /setactive {prior ?? "381b4222-f694-41f0-9685-ff5bb260df2e"}",
        },
        new ScheduledTaskStateTweak
        {
            Id = "win.task.compat",
            Name = "Desactivar Microsoft Compatibility Appraiser",
            Description = "Tarea programada que escanea el equipo en segundo plano para telemetría de compatibilidad.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            TaskPath = @"\Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser",
        },
        new ScheduledTaskStateTweak
        {
            Id = "win.task.ceip",
            Name = "Desactivar Customer Experience Improvement Program",
            Description = "Tarea de recolección de estadísticas de uso para el programa CEIP de Microsoft.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            TaskPath = @"\Microsoft\Windows\Customer Experience Improvement Program\Consolidator",
        },
        new ShellCommandTweak
        {
            Id = "win.powerplan.ultimate",
            Name = "Plan de energía: Rendimiento máximo (Ultimate Performance)",
            Description = "Activa el plan oculto 'Rendimiento máximo' de Windows: va más allá de Alto rendimiento eliminando aún más límites de ahorro de energía. Solo recomendable en equipos de escritorio conectados a la corriente.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            CaptureState = () => ExtractGuid(ShellCommandTweak.ExecCapture("powercfg /getactivescheme")),
            ApplyCommand = () =>
                "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 e1a2b3c4-9c77-4c9a-8a5a-2f2f2f2f2f2f >nul 2>&1 & powercfg /setactive e1a2b3c4-9c77-4c9a-8a5a-2f2f2f2f2f2f",
            RevertCommand = prior => $"powercfg /setactive {prior ?? "381b4222-f694-41f0-9685-ff5bb260df2e"}",
        },
        new ShellCommandTweak
        {
            Id = "win.coreunpark",
            Name = "Desactivar Core Parking (CPU)",
            Description = "Impide que Windows 'estacione' núcleos de CPU inactivos, para que todos respondan de inmediato bajo carga variable como en juegos.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            CaptureState = () => CaptureCoreParkingState(),
            ApplyCommand = () =>
                "powercfg /setacvalueindex scheme_current sub_processor 0cc5b647-c1df-4637-891a-dec35c318583 100 & " +
                "powercfg /setdcvalueindex scheme_current sub_processor 0cc5b647-c1df-4637-891a-dec35c318583 100 & " +
                "powercfg /setactive scheme_current",
            RevertCommand = prior => RestoreCoreParkingCommand(prior),
        },
        new RegistryTweak
        {
            Id = "win.cortana",
            Name = "Desactivar Cortana",
            Description = "Impide que Cortana se ejecute y consuma recursos en segundo plano.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search",
            ValueName = "AllowCortana",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "win.activityhistory",
            Name = "Desactivar historial de actividades (Timeline)",
            Description = "Evita que Windows registre y suba tu actividad reciente entre dispositivos.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\System",
            ValueName = "EnableActivityFeed",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "win.advertisingid",
            Name = "Desactivar ID de publicidad",
            Description = "Impide que las apps usen tu ID de publicidad para personalizar anuncios entre aplicaciones.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo",
            ValueName = "Enabled",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "win.location",
            Name = "Desactivar rastreo de ubicación",
            Description = "Deniega el acceso general a la ubicación del dispositivo para todas las apps.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location",
            ValueName = "Value",
            EnabledValue = "Deny",
            ValueKind = RegistryValueKind.String,
        },
        new ShellCommandTweak
        {
            Id = "win.hpet",
            Name = "Desactivar reloj de plataforma forzado (HPET)",
            Description = "Deja que Windows elija su propio temporizador en vez de forzar el HPET, lo que en muchos equipos reduce el overhead de temporización. Requiere reinicio.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            CaptureState = () =>
            {
                var line = ShellCommandTweak.ExecCapture("bcdedit /enum {current}")
                    .Split('\n')
                    .FirstOrDefault(l => l.Contains("useplatformclock", StringComparison.OrdinalIgnoreCase))
                    ?.Trim();
                if (line is null) return null; // key absent = Windows picks the clock itself (the default we're restoring to)
                return line.Contains("Yes", StringComparison.OrdinalIgnoreCase) ? "true" : "false";
            },
            ApplyCommand = () => "bcdedit /deletevalue useplatformclock",
            RevertCommand = prior => prior is null
                ? "bcdedit /deletevalue useplatformclock"
                : $"bcdedit /set useplatformclock {prior}",
        },
        new RegistryTweak
        {
            Id = "win.transparency",
            Name = "Desactivar efectos de transparencia",
            Description = "Desactiva transparencias de Windows para reducir composición visual en equipos modestos.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
            ValueName = "EnableTransparency",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "win.backgroundapps",
            Name = "Restringir apps en segundo plano",
            Description = "Evita que aplicaciones UWP se ejecuten en segundo plano. Puede retrasar notificaciones de esas apps.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
            ValueName = "GlobalUserDisabled",
            EnabledValue = 1,
        },
        new ServiceStateTweak
        {
            Id = "win.errorreporting",
            Name = "Desactivar informes de errores de Windows",
            Description = "Deshabilita Windows Error Reporting (WerSvc). No se enviarán informes automáticos de fallos a Microsoft.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "WerSvc",
            DesiredStartMode = "Disabled",
        },
        new ServiceStateTweak
        {
            Id = "win.searchindex",
            Name = "Desactivar indexación de búsqueda",
            Description = "Deshabilita Windows Search para reducir actividad en disco; las búsquedas del menú Inicio pueden tardar más.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            ServiceName = "WSearch",
            DesiredStartMode = "Disabled",
        },
        new RegistryTweak
        {
            Id = "win.startsearch.web",
            Name = "Desactivar sugerencias web en Inicio",
            Description = "Desactiva las sugerencias web/Bing en la búsqueda del menú Inicio.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Software\Policies\Microsoft\Windows\Explorer",
            ValueName = "DisableSearchBoxSuggestions",
            EnabledValue = 1,
        },
        new RegistryTweak
        {
            Id = "win.powerthrottling",
            Name = "Desactivar Power Throttling",
            Description = "Evita que Windows reduzca agresivamente recursos de procesos en segundo plano. Aumenta el consumo energético.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling",
            ValueName = "PowerThrottlingOff",
            EnabledValue = 1,
        },
        new PowerPlanSettingTweak
        {
            Id = "win.pcieaspm",
            Name = "Desactivar ahorro de energía PCI Express",
            Description = "Desactiva Link State Power Management del plan activo en CA y batería. Puede ayudar con latencia, a costa de energía.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Subgroup = "SUB_PCIEXPRESS",
            Setting = "ASPM",
            EnabledValue = "0x00000000",
        },
        new RegistryTweak { Id = "win.programtracking", Name = "Desactivar seguimiento de programas", Description = "Evita que Inicio registre los programas usados con frecuencia.", Category = TweakCategory.Windows, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", ValueName = "Start_TrackProgs", EnabledValue = 0 },
        new RegistryTweak { Id = "win.telemetry.appdata", Name = "Limitar recopilación de datos de apps", Description = "Desactiva la directiva AllowAppDataCollection.", Category = TweakCategory.Windows, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.LocalMachine, SubKey = @"SOFTWARE\Policies\Microsoft\Windows\System", ValueName = "AllowAppDataCollection", EnabledValue = 0 },
        new RegistryTweak { Id = "win.telemetry.advertising", Name = "Bloquear publicidad de Windows", Description = "Desactiva la directiva de identificadores publicitarios de Windows.", Category = TweakCategory.Windows, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.LocalMachine, SubKey = @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", ValueName = "DisableWindowsAdvertising", EnabledValue = 1 },
        new RegistryTweak { Id = "win.telemetry.consumer", Name = "Desactivar experiencia de consumidor", Description = "Evita sugerencias y aplicaciones promocionales de Microsoft.", Category = TweakCategory.Windows, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.LocalMachine, SubKey = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", ValueName = "DisableMicrosoftConsumerExperience", EnabledValue = 1 },
        new ServiceStateTweak { Id = "win.biometric", Name = "Desactivar servicio biométrico", Description = "Deshabilita Windows Biometric Service. No lo uses si utilizás Windows Hello.", Category = TweakCategory.Windows, Risk = RiskLevel.Advanced, DefaultEnabled = false, ServiceName = "WbioSrvc", DesiredStartMode = "Disabled" },
        new ServiceStateTweak { Id = "win.printspooler", Name = "Desactivar cola de impresión", Description = "Deshabilita Print Spooler. No lo uses si imprimís o usás impresoras virtuales.", Category = TweakCategory.Windows, Risk = RiskLevel.Advanced, DefaultEnabled = false, ServiceName = "Spooler", DesiredStartMode = "Disabled" },
        new RegistryTweak { Id = "win.prefetch", Name = "Desactivar Prefetch", Description = "Desactiva Prefetch. Solo recomendado para diagnosticar actividad persistente de disco en equipos concretos.", Category = TweakCategory.Windows, Risk = RiskLevel.Advanced, DefaultEnabled = false, Hive = RegistryHive.LocalMachine, SubKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", ValueName = "EnablePrefetcher", EnabledValue = 0 },

        // Tiempos de espera del sistema. Windows los lee como texto (REG_SZ), no como número:
        // escribirlos como DWORD deja el valor puesto pero sin efecto.
        new RegistryTweak
        {
            Id = "win.waittokillapp",
            Name = "Cerrar apps más rápido al apagar",
            Description = "Baja a 2 segundos la espera antes de forzar el cierre de un programa al apagar el equipo (por defecto 5).",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Desktop",
            ValueName = "WaitToKillAppTimeout",
            EnabledValue = "2000",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "win.hungapptimeout",
            Name = "Detectar antes las apps colgadas",
            Description = "Windows considera que un programa dejó de responder tras 1 segundo en vez de 5.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Desktop",
            ValueName = "HungAppTimeout",
            EnabledValue = "1000",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "win.autoendtasks",
            Name = "Cerrar solo las apps colgadas al apagar",
            Description = "Evita que el apagado se quede esperando por un programa que no responde.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Desktop",
            ValueName = "AutoEndTasks",
            EnabledValue = "1",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "win.waittokillservice",
            Name = "Cerrar servicios más rápido al apagar",
            Description = "Baja a 2 segundos la espera por cada servicio al apagar (por defecto 5).",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control",
            ValueName = "WaitToKillServiceTimeout",
            EnabledValue = "2000",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "win.foregroundlock",
            Name = "Permitir que las apps pasen al frente",
            Description = "Quita el bloqueo que impide a un programa robar el foco, útil cuando un juego tarda en pasar a primer plano.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Desktop",
            ValueName = "ForegroundLockTimeout",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "win.pagingexecutive",
            Name = "Mantener el kernel en RAM",
            Description = "Impide que Windows mande el núcleo y los drivers al archivo de paginación. Consume más RAM: úsalo solo con 16 GB o más.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
            ValueName = "DisablePagingExecutive",
            EnabledValue = 1,
        },
        new RegistryTweak
        {
            Id = "win.timerresolution",
            Name = "Resolución global del temporizador",
            Description = "Aplica a todo el sistema la resolución de temporizador que pida un programa. Puede subir el consumo en portátiles.",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\kernel",
            ValueName = "GlobalTimerResolutionRequests",
            EnabledValue = 1,
        },
        new RegistryTweak
        {
            Id = "win.prioritycontrol",
            Name = "Más CPU para la ventana activa",
            Description = "Reparte el tiempo de CPU favoreciendo al programa en primer plano (Win32PrioritySeparation = 38).",
            Category = TweakCategory.Windows,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control\PriorityControl",
            ValueName = "Win32PrioritySeparation",
            EnabledValue = 38,
        },
    };

    private static string? ExtractGuid(string powercfgOutput)
    {
        // "Esquema de energía activo actual: 8c5e7fda-... (Alto rendimiento)"
        var start = powercfgOutput.IndexOf(':');
        if (start < 0) return null;
        var rest = powercfgOutput[(start + 1)..].Trim();
        var end = rest.IndexOf(' ');
        return end > 0 ? rest[..end] : rest;
    }

    private const string CoreParkingSubGuid = "0cc5b647-c1df-4637-891a-dec35c318583";

    private static string? CaptureCoreParkingState()
    {
        var output = ShellCommandTweak.ExecCapture($"powercfg /q scheme_current sub_processor {CoreParkingSubGuid}");
        var ac = System.Text.RegularExpressions.Regex.Match(output, @"Current AC Power Setting Index:\s*(0x[0-9a-fA-F]+)").Groups[1].Value;
        var dc = System.Text.RegularExpressions.Regex.Match(output, @"Current DC Power Setting Index:\s*(0x[0-9a-fA-F]+)").Groups[1].Value;
        if (string.IsNullOrEmpty(ac)) ac = "0x00000064";
        if (string.IsNullOrEmpty(dc)) dc = "0x00000064";
        return $"{ac}|{dc}";
    }

    private static string RestoreCoreParkingCommand(string? prior)
    {
        var parts = (prior ?? "0x00000064|0x00000064").Split('|');
        var ac = parts.ElementAtOrDefault(0) ?? "0x00000064";
        var dc = parts.ElementAtOrDefault(1) ?? "0x00000064";
        return $"powercfg /setacvalueindex scheme_current sub_processor {CoreParkingSubGuid} {ac} & " +
               $"powercfg /setdcvalueindex scheme_current sub_processor {CoreParkingSubGuid} {dc} & " +
               "powercfg /setactive scheme_current";
    }
}
