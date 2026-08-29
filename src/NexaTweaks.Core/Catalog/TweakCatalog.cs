using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

/// <summary>Static, declarative catalog of every built-in tweak, grouped by category.</summary>
public static class TweakCatalog
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
    };

    public static IReadOnlyList<ITweak> Network { get; } = new List<ITweak>
    {
        new NagleTweak
        {
            Id = "net.nagle",
            Name = "Desactivar algoritmo de Nagle",
            Description = "TcpAckFrequency=1 y TCPNoDelay=1 en todas las interfaces: reduce la latencia de paquetes pequeños.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
        },
        new RegistryTweak
        {
            Id = "net.throttling",
            Name = "Desactivar Network Throttling Index",
            Description = "Elimina el límite que Windows aplica al procesamiento de red para dar prioridad a multimedia/juegos.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
            ValueName = "NetworkThrottlingIndex",
            EnabledValue = unchecked((int)0xffffffff),
        },
        new RegistryTweak
        {
            Id = "net.qos",
            Name = "Liberar ancho de banda reservado por QoS",
            Description = "Pone a 0 el porcentaje que el planificador de paquetes QoS reserva por defecto (normalmente 20%).",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\Psched",
            ValueName = "NonBestEffortLimit",
            EnabledValue = 0,
        },
        new ShellCommandTweak
        {
            Id = "net.dns",
            Name = "DNS rápido (Cloudflare 1.1.1.1)",
            Description = "Cambia el DNS del adaptador activo a Cloudflare (1.1.1.1 / 1.0.0.1), normalmente más rápido que el del ISP.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            CaptureState = () => ShellCommandTweak.ExecCapture(
                "powershell -NoProfile -Command \"(Get-DnsClientServerAddress -AddressFamily IPv4 | Where-Object {$_.ServerAddresses.Count -gt 0} | Select-Object -First 1).InterfaceAlias\"").Trim(),
            ApplyCommand = () =>
                "powershell -NoProfile -Command \"$i=(Get-NetAdapter | Where-Object Status -eq 'Up' | Select-Object -First 1).Name; Set-DnsClientServerAddress -InterfaceAlias $i -ServerAddresses 1.1.1.1,1.0.0.1\"",
            RevertCommand = prior =>
                $"powershell -NoProfile -Command \"Set-DnsClientServerAddress -InterfaceAlias '{prior}' -ResetServerAddresses\"",
        },
        new ShellCommandTweak
        {
            Id = "net.adapterpower",
            Name = "Desactivar ahorro de energía de la red",
            Description = "Evita que Windows apague el adaptador de red para ahorrar energía, una causa común de micro-cortes y picos de ping intermitentes.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            CaptureState = () => ShellCommandTweak.ExecCapture(
                "powershell -NoProfile -Command \"(Get-NetAdapterPowerManagement | ForEach-Object { $_.Name + '=' + $_.AllowComputerToTurnOffDevice }) -join ';'\"").Trim(),
            ApplyCommand = () =>
                "powershell -NoProfile -Command \"Get-NetAdapter | Disable-NetAdapterPowerManagement -ErrorAction SilentlyContinue\"",
            RevertCommand = prior =>
            {
                if (string.IsNullOrWhiteSpace(prior)) return "cmd /c exit 0";
                var namesToRestore = prior.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => p.EndsWith("=Enabled", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p[..p.LastIndexOf('=')])
                    .ToList();
                if (namesToRestore.Count == 0) return "cmd /c exit 0";
                var nameList = string.Join(",", namesToRestore.Select(n => $"'{n}'"));
                return $"powershell -NoProfile -Command \"Enable-NetAdapterPowerManagement -Name {nameList} -ErrorAction SilentlyContinue\"";
            },
        },
    };

    public static IReadOnlyList<ITweak> Input { get; } = new List<ITweak>
    {
        new RegistryTweak
        {
            Id = "input.mousespeed",
            Name = "Desactivar aceleración del mouse",
            Description = "MouseSpeed=0: movimiento 1:1 sin 'Enhance pointer precision', igual que piden los jugadores competitivos.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Mouse",
            ValueName = "MouseSpeed",
            EnabledValue = "0",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "input.mousethreshold1",
            Name = "Anular umbral de aceleración 1",
            Description = "MouseThreshold1=0, necesario junto con MouseSpeed para eliminar la curva de aceleración por completo.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Mouse",
            ValueName = "MouseThreshold1",
            EnabledValue = "0",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "input.mousethreshold2",
            Name = "Anular umbral de aceleración 2",
            Description = "MouseThreshold2=0, completa la desactivación de la aceleración de puntero de Windows.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Mouse",
            ValueName = "MouseThreshold2",
            EnabledValue = "0",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "input.keyboardspeed",
            Name = "Teclado: velocidad de repetición máxima",
            Description = "KeyboardSpeed=31 (máximo permitido por Windows) para respuesta de tecla mantenida más rápida.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Control Panel\Keyboard",
            ValueName = "KeyboardSpeed",
            EnabledValue = "31",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "input.usbselectivesuspend",
            Name = "Desactivar suspensión selectiva de USB",
            Description = "Evita que Windows suspenda puertos USB inactivos, para que mouse/teclado respondan sin 'despertar'.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\USB",
            ValueName = "DisableSelectiveSuspend",
            EnabledValue = 1,
        },
        new PowerPlanSettingTweak { Id = "input.usbselectivesuspend.power", Name = "Desactivar suspensión USB del plan activo", Description = "Desactiva USB selective suspend tanto en CA como en batería y conserva los valores anteriores para restaurarlos.", Category = TweakCategory.Input, Risk = RiskLevel.Advanced, DefaultEnabled = false, Subgroup = "SUB_USB", Setting = "USBSELECTIVE", EnabledValue = "0x00000000" },
        new RegistryTweak
        {
            Id = "input.mousequeuesize",
            Name = "Reducir cola de datos del mouse",
            Description = "Baja MouseDataQueueSize de 100 (por defecto) a 20, para que Windows procese cada movimiento con menos buffer entre medio. Requiere reinicio.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\mouclass\Parameters",
            ValueName = "MouseDataQueueSize",
            EnabledValue = 20,
        },
        new RegistryTweak
        {
            Id = "input.keyboardqueuesize",
            Name = "Reducir cola de datos del teclado",
            Description = "Baja KeyboardDataQueueSize de 100 (por defecto) a 20, mismo efecto que la cola del mouse pero para el teclado. Requiere reinicio.",
            Category = TweakCategory.Input,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters",
            ValueName = "KeyboardDataQueueSize",
            EnabledValue = 20,
        },
    };

    public static IReadOnlyList<ITweak> Gpu { get; } = new List<ITweak>
    {
        new RegistryTweak
        {
            Id = "gpu.hags",
            Name = "Hardware-Accelerated GPU Scheduling",
            Description = "Delega la gestión de la cola de la GPU al propio hardware. Requiere reinicio para notarse.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
            ValueName = "HwSchMode",
            EnabledValue = 2,
        },
        new RegistryTweak
        {
            Id = "gpu.gamedvr.policy",
            Name = "Desactivar Xbox Game Bar / Game DVR",
            Description = "Apaga la grabación en segundo plano de Game Bar, que puede robar FPS durante la partida.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Policies\Microsoft\Windows\GameDVR",
            ValueName = "AllowGameDVR",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "gpu.gamedvr.user",
            Name = "Desactivar captura en segundo plano (usuario)",
            Description = "GameDVR_Enabled=0 a nivel de usuario, complementa la directiva global para desactivar Game DVR del todo.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"System\GameConfigStore",
            ValueName = "GameDVR_Enabled",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "gpu.responsiveness",
            Name = "Prioridad multimedia/juegos máxima",
            Description = "SystemResponsiveness=0: cede al primer plano (juego) el CPU que Windows reserva para tareas en segundo plano.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
            ValueName = "SystemResponsiveness",
            EnabledValue = 0,
        },
        new RegistryTweak
        {
            Id = "gpu.tasks.priority",
            Name = "Prioridad de GPU máxima para juegos",
            Description = "GPU Priority=8 en el perfil de tareas 'Games' de Windows, para que el planificador priorice el proceso del juego.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
            ValueName = "GPU Priority",
            EnabledValue = 8,
        },
        new RegistryTweak
        {
            Id = "gpu.mpo",
            Name = "Desactivar Multi-Plane Overlay (MPO)",
            Description = "Desactiva MPO, útil solo si sufrís parpadeos, stutter o problemas de overlays. Requiere reinicio.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows\Dwm",
            ValueName = "OverlayTestMode",
            EnabledValue = 5,
        },
        new RegistryTweak
        {
            Id = "gpu.fullscreenoptimizations",
            Name = "Desactivar optimizaciones de pantalla completa",
            Description = "Fuerza el comportamiento clásico de pantalla completa para juegos con problemas de input lag o stutter.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"System\GameConfigStore",
            ValueName = "GameDVR_FSEBehaviorMode",
            EnabledValue = 2,
        },
        new RegistryTweak
        {
            Id = "gpu.games.priority",
            Name = "Prioridad de CPU alta para juegos",
            Description = "Configura la prioridad de CPU del perfil multimedia Games en 6.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
            ValueName = "Priority",
            EnabledValue = 6,
        },
        new RegistryTweak
        {
            Id = "gpu.games.scheduling",
            Name = "Planificador multimedia: Games alto",
            Description = "Configura Scheduling Category=High en el perfil multimedia Games.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
            ValueName = "Scheduling Category",
            EnabledValue = "High",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak
        {
            Id = "gpu.games.sfio",
            Name = "Prioridad I/O alta para juegos",
            Description = "Configura SFIO Priority=High en el perfil multimedia Games.",
            Category = TweakCategory.Gpu,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
            ValueName = "SFIO Priority",
            EnabledValue = "High",
            ValueKind = RegistryValueKind.String,
        },
        new RegistryTweak { Id = "gpu.gamebar.startup", Name = "Ocultar inicio de Game Bar", Description = "Evita que Xbox Game Bar muestre su panel al iniciar un juego.", Category = TweakCategory.Gpu, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"Software\Microsoft\GameBar", ValueName = "ShowStartupPanel", EnabledValue = 0 },
        new RegistryTweak { Id = "gpu.gamebar.nexus", Name = "Desactivar interfaz Game Bar", Description = "Desactiva la interfaz Nexus de Xbox Game Bar para el usuario actual.", Category = TweakCategory.Gpu, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"Software\Microsoft\GameBar", ValueName = "UseNexusForGameBarEnabled", EnabledValue = 0 },
        new RegistryTweak { Id = "gpu.gamebar.capture", Name = "Desactivar capturas de Game DVR", Description = "Desactiva AppCaptureEnabled para evitar grabaciones/capturas en segundo plano.", Category = TweakCategory.Gpu, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"Software\Microsoft\Windows\CurrentVersion\GameDVR", ValueName = "AppCaptureEnabled", EnabledValue = 0 },
        new RegistryTweak { Id = "gpu.gamemode.auto", Name = "Activar Game Mode automático", Description = "Activa AutoGameModeEnabled para el usuario actual.", Category = TweakCategory.Gpu, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"Software\Microsoft\GameBar", ValueName = "AutoGameModeEnabled", EnabledValue = 1 },
        new RegistryTweak { Id = "gpu.gamemode.allow", Name = "Permitir Game Mode", Description = "Permite Game Mode para el usuario actual.", Category = TweakCategory.Gpu, Risk = RiskLevel.Safe, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"Software\Microsoft\GameBar", ValueName = "AllowAutoGameMode", EnabledValue = 1 },
        new RegistryTweak { Id = "gpu.fullscreenoptimizations.honor", Name = "Forzar comportamiento de pantalla completa", Description = "Complementa el ajuste de fullscreen para que Game DVR respete la preferencia del usuario.", Category = TweakCategory.Gpu, Risk = RiskLevel.Advanced, DefaultEnabled = false, Hive = RegistryHive.CurrentUser, SubKey = @"System\GameConfigStore", ValueName = "GameDVR_HonorUserFSEBehaviorMode", EnabledValue = 1 },
        new RegistryTweak { Id = "gpu.foregroundpriority", Name = "Priorizar tareas en primer plano", Description = "Ajusta Win32PrioritySeparation=38 para favorecer la aplicación en primer plano. Requiere reinicio.", Category = TweakCategory.Gpu, Risk = RiskLevel.Advanced, DefaultEnabled = false, RequiresRestart = true, Hive = RegistryHive.LocalMachine, SubKey = @"SYSTEM\CurrentControlSet\Control\PriorityControl", ValueName = "Win32PrioritySeparation", EnabledValue = 38 },
    };

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
