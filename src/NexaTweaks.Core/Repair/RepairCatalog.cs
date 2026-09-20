namespace NexaTweaks.Core.Repair;

/// <summary>Catalog of built-in Windows repair/reset tools, inspired by the "repair center" both
/// EXM Tweaks and ElmaxiShark's Optimizer PRO ship and NexaTweaks was missing.</summary>
public static class RepairCatalog
{
    public static IReadOnlyList<RepairAction> Actions { get; } = new List<RepairAction>
    {
        new()
        {
            Id = "repair.sfc",
            Name = "System File Checker (SFC)",
            Description = "Verifica la integridad de los archivos de sistema y repara los que estén dañados o modificados.",
            Risk = RiskLevel.Safe,
            Executable = "sfc.exe",
            Arguments = "/scannow",
            EstimatedDuration = "5-15 min",
        },
        new()
        {
            Id = "repair.dism",
            Name = "Reparar imagen de Windows (DISM)",
            Description = "Repara la imagen base que usa SFC. Ejecútalo si SFC no logra corregir todos los errores.",
            Risk = RiskLevel.Safe,
            Executable = "DISM.exe",
            Arguments = "/Online /Cleanup-Image /RestoreHealth",
            EstimatedDuration = "5-20 min",
        },
        new()
        {
            Id = "repair.chkdsk",
            Name = "Comprobar disco del sistema (CHKDSK)",
            Description = "Programa un análisis de errores del disco C: para el próximo reinicio. El equipo reiniciará más lento esa vez.",
            Risk = RiskLevel.Advanced,
            Executable = "chkdsk.exe",
            Arguments = "C: /f /r",
            RequiresConfirmationInput = true,
            EstimatedDuration = "Se aplica en el próximo reinicio",
            RequiresRestart = true,
        },
        new()
        {
            Id = "repair.winsock",
            Name = "Restablecer Winsock",
            Description = "Reinicia el catálogo de Winsock. Soluciona errores de conexión tras desinstalar VPNs o software de red.",
            Risk = RiskLevel.Safe,
            Executable = "netsh.exe",
            Arguments = "winsock reset",
            EstimatedDuration = "Instantáneo (requiere reiniciar)",
            RequiresRestart = true,
        },
        new()
        {
            Id = "repair.tcpip",
            Name = "Restablecer TCP/IP",
            Description = "Reinicia la pila TCP/IP a su configuración de fábrica.",
            Risk = RiskLevel.Safe,
            Executable = "netsh.exe",
            Arguments = "int ip reset",
            EstimatedDuration = "Instantáneo (requiere reiniciar)",
            RequiresRestart = true,
        },
        new()
        {
            Id = "repair.dns",
            Name = "Vaciar caché DNS",
            Description = "Limpia la caché de resolución DNS local. Útil cuando una web no carga tras cambiar de servidor.",
            Risk = RiskLevel.Safe,
            Executable = "ipconfig.exe",
            Arguments = "/flushdns",
            EstimatedDuration = "Instantáneo",
        },
        new()
        {
            Id = "repair.windowsupdate",
            Name = "Restablecer Windows Update",
            Description = "Detiene los servicios de Update, renombra sus cachés (SoftwareDistribution/catroot2) y los reinicia. Soluciona actualizaciones atascadas.",
            Risk = RiskLevel.Advanced,
            Executable = "cmd.exe",
            Arguments = "/c net stop wuauserv & net stop bits & net stop cryptsvc & ren %systemroot%\\SoftwareDistribution SoftwareDistribution.bak & ren %systemroot%\\System32\\catroot2 catroot2.bak & net start cryptsvc & net start bits & net start wuauserv",
            EstimatedDuration = "~30 seg",
        },
        new()
        {
            Id = "repair.store",
            Name = "Restablecer Microsoft Store",
            Description = "Limpia la caché de la Store en caso de errores de descarga o licencias.",
            Risk = RiskLevel.Safe,
            Executable = "wsreset.exe",
            Arguments = "",
            EstimatedDuration = "Instantáneo",
        },
        new()
        {
            Id = "repair.bluetooth",
            Name = "Reactivar Bluetooth",
            Description = "Configura y arranca el servicio Bluetooth Support Service si quedó detenido o deshabilitado.",
            Risk = RiskLevel.Safe,
            Executable = "cmd.exe",
            Arguments = "/c sc config bthserv start= auto & sc start bthserv",
            EstimatedDuration = "Instantáneo",
        },
        new()
        {
            Id = "repair.wifi",
            Name = "Reactivar Wi-Fi",
            Description = "Reactiva los servicios de red esenciales y restablece Winsock/TCP-IP. Requiere reiniciar.",
            Risk = RiskLevel.Advanced,
            Executable = "cmd.exe",
            Arguments = "/c sc config WlanSvc start= auto & sc start WlanSvc & sc config Dhcp start= auto & sc start Dhcp & sc config NlaSvc start= auto & sc start NlaSvc & netsh winsock reset & netsh int ip reset",
            EstimatedDuration = "~30 seg (requiere reiniciar)",
            RequiresRestart = true,
        },
        new()
        {
            Id = "repair.audio",
            Name = "Reiniciar servicios de audio",
            Description = "Reactiva y reinicia Windows Audio y Audio Endpoint Builder para recuperar salida o entrada de audio.",
            Risk = RiskLevel.Safe,
            Executable = "cmd.exe",
            Arguments = "/c sc config AudioEndpointBuilder start= auto & sc config Audiosrv start= auto & net stop Audiosrv & net start AudioEndpointBuilder & net start Audiosrv",
            EstimatedDuration = "~15 seg",
        },
        new()
        {
            Id = "repair.airplane",
            Name = "Reparar modo avión",
            Description = "Reactiva Radio Management y WLAN AutoConfig, los servicios principales para salir de modo avión en notebooks.",
            Risk = RiskLevel.Safe,
            Executable = "cmd.exe",
            Arguments = "/c sc config RmSvc start= auto & sc start RmSvc & sc config WlanSvc start= auto & sc start WlanSvc",
            EstimatedDuration = "Instantáneo",
        },
        new()
        {
            Id = "repair.cleanmgr",
            Name = "Liberar espacio en disco",
            Description = "Abre el Liberador de espacio de Windows con todas las categorías marcadas (archivos temporales, actualizaciones antiguas, papelera).",
            Risk = RiskLevel.Safe,
            Executable = "cleanmgr.exe",
            Arguments = "/verylowdisk",
            EstimatedDuration = "2-10 min",
        },
        new()
        {
            Id = "repair.temp",
            Name = "Vaciar archivos temporales",
            Description = "Borra el contenido de la carpeta TEMP del usuario y del sistema. Los archivos en uso se omiten.",
            Risk = RiskLevel.Safe,
            Executable = "cmd.exe",
            Arguments = "/c del /q /f /s \"%TEMP%\\*\" & del /q /f /s \"%SystemRoot%\\Temp\\*\"",
            EstimatedDuration = "~30 seg",
        },
        new()
        {
            Id = "repair.recyclebin",
            Name = "Vaciar la papelera de reciclaje",
            Description = "Elimina definitivamente lo que haya en la papelera de todas las unidades.",
            Risk = RiskLevel.Advanced,
            Executable = "powershell.exe",
            Arguments = "-NoProfile -Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"",
            EstimatedDuration = "~15 seg",
        },
        new()
        {
            Id = "repair.network.full",
            Name = "Restablecer la red por completo",
            Description = "Reinicia Winsock y TCP/IP, vacía la caché DNS y renueva la IP. Deja la conexión como recién instalada.",
            Risk = RiskLevel.Advanced,
            Executable = "cmd.exe",
            Arguments = "/c netsh winsock reset & netsh int ip reset & ipconfig /flushdns & ipconfig /release & ipconfig /renew",
            EstimatedDuration = "~30 seg (requiere reiniciar)",
            RequiresRestart = true,
        },
        new()
        {
            Id = "repair.spooler",
            Name = "Reiniciar la cola de impresión",
            Description = "Detiene la cola, borra los trabajos atascados y la vuelve a arrancar. Para cuando una impresión se queda colgada.",
            Risk = RiskLevel.Safe,
            Executable = "cmd.exe",
            Arguments = "/c net stop spooler & del /q /f /s \"%SystemRoot%\\System32\\spool\\PRINTERS\\*\" & net start spooler",
            EstimatedDuration = "~15 seg",
        },
        new()
        {
            Id = "repair.explorer",
            Name = "Reiniciar Explorador y barra de tareas",
            Description = "Reinicia explorer.exe para reparar menú Inicio, barra de tareas o escritorio sin reiniciar Windows.",
            Risk = RiskLevel.Safe,
            Executable = "cmd.exe",
            Arguments = "/c taskkill /f /im explorer.exe & start explorer.exe",
            EstimatedDuration = "Instantáneo",
        },
    };
}
