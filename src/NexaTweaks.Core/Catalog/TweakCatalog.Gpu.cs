using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
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
}
