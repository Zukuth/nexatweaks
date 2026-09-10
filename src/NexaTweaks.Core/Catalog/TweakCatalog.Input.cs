using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
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
}
