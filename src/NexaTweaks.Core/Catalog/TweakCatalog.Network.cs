using Microsoft.Win32;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
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

        new RegistryTweak
        {
            Id = "net.tcp.pmtu",
            Name = "Detección automática de MTU",
            Description = "Deja que Windows calcule el tamaño de paquete óptimo por ruta en vez de usar 576 bytes fijos.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = true,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
            ValueName = "EnablePMTUDiscovery",
            EnabledValue = 1,
        },
        new RegistryTweak
        {
            Id = "net.tcp.retransmissions",
            Name = "Reintentos TCP más cortos",
            Description = "Baja a 5 los reintentos antes de dar una conexión por perdida, para que los cortes se noten menos.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters",
            ValueName = "TcpMaxDataRetransmissions",
            EnabledValue = 5,
        },
        new RegistryTweak
        {
            Id = "net.dnspriority",
            Name = "Priorizar respuestas DNS",
            Description = "Windows consulta antes el DNS que otros métodos de resolución más lentos (NetBIOS).",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider",
            ValueName = "DnsPriority",
            EnabledValue = 6,
        },
        new RegistryTweak
        {
            Id = "net.localpriority",
            Name = "Priorizar la caché y el archivo hosts",
            Description = "Resuelve primero con lo que ya está en el equipo antes de salir a la red.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Safe,
            DefaultEnabled = false,
            RequiresRestart = true,
            Hive = RegistryHive.LocalMachine,
            SubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider",
            ValueName = "LocalPriority",
            EnabledValue = 4,
        },
        new ShellCommandTweak
        {
            Id = "net.teredo",
            Name = "Desactivar Teredo",
            Description = "Apaga el túnel IPv6 sobre IPv4 que Windows usa para algunas conexiones P2P. Suele sobrar y añade latencia.",
            Category = TweakCategory.Network,
            Risk = RiskLevel.Advanced,
            DefaultEnabled = false,
            CaptureState = () =>
            {
                var output = ShellCommandTweak.ExecCapture("netsh interface teredo show state");
                return output.Contains("disabled", StringComparison.OrdinalIgnoreCase) ? "disabled" : "default";
            },
            ApplyCommand = () => "netsh interface teredo set state disabled",
            RevertCommand = prior => prior == "disabled"
                ? "netsh interface teredo set state disabled"
                : "netsh interface teredo set state default",
            AppliedCheck = () => ShellCommandTweak.ExecCapture("netsh interface teredo show state")
                .Contains("disabled", StringComparison.OrdinalIgnoreCase),
        },
    };
}
