using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Catalog;

public static partial class TweakCatalog
{
    /// <summary>
    /// Windows services that a home or gaming PC rarely needs. Deliberately excluded: Windows
    /// Update (wuauserv, WaaSMedicSvc, BITS, CryptSvc), Defender and the firewall - breaking those
    /// leaves the machine unpatched or unprotected, which is never worth a few MB of RAM.
    /// Everything here is reversible: the previous start mode is saved before changing it.
    /// </summary>
    public static IReadOnlyList<ITweak> Services { get; } = new List<ITweak>
    {
        Service("svc.remoteregistry", "RemoteRegistry", "Registro remoto",
            "Permite que otro equipo edite tu registro por la red. En un PC doméstico no hace falta y es una puerta menos.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.fax", "Fax", "Fax",
            "Servicio de envío y recepción de faxes. Solo sirve si tienes un módem de fax conectado.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.retaildemo", "RetailDemo", "Modo demostración de tienda",
            "Modo que usan las tiendas para exponer el equipo. Nunca se usa en casa.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.mapsbroker", "MapsBroker", "Descarga de mapas sin conexión",
            "Descarga y actualiza mapas en segundo plano para la app Mapas.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.wmpnetworksvc", "WMPNetworkSvc", "Uso compartido de Windows Media Player",
            "Comparte tu biblioteca multimedia con otros equipos de la red.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.p2psvc", "p2psvc", "Agrupación en el hogar (P2P)",
            "Red entre pares que usaban Grupo Hogar y Asistencia rápida antigua.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.p2pimsvc", "p2pimsvc", "Administrador de identidad P2P",
            "Identidades para la red entre pares. Va de la mano del servicio anterior.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.pnrpsvc", "PNRPsvc", "Protocolo de resolución de nombres entre pares",
            "Resuelve nombres en redes P2P sin servidor DNS.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.pnrpautoreg", "PNRPAutoReg", "Registro automático PNRP",
            "Publica este equipo en la red entre pares.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.ajrouter", "AJRouter", "Enrutador AllJoyn",
            "Comunicación con dispositivos domóticos por el estándar AllJoyn, hoy en desuso.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.wersvc", "WerSvc", "Informe de errores de Windows",
            "Envía informes de fallos a Microsoft. Apagarlo deja de enviar datos, pero los volcados locales siguen creándose.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.pcasvc", "PcaSvc", "Asistente de compatibilidad de programas",
            "Detecta programas antiguos y ofrece ejecutarlos en modo compatibilidad. Recopila datos de uso.",
            RiskLevel.Advanced),

        Service("svc.cscservice", "CscService", "Archivos sin conexión",
            "Guarda copias locales de carpetas de red compartidas. Propio de empresas con servidor.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.snmptrap", "SNMPTRAP", "Captura SNMP",
            "Recibe mensajes de gestión de red SNMP. Solo se usa en redes administradas.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.lltdsvc", "lltdsvc", "Topología de red (LLTD)",
            "Dibuja el mapa de dispositivos de la red local.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.rpclocator", "RpcLocator", "Localizador de llamadas a procedimiento remoto",
            "Servicio heredado de RPC de Windows NT. No es el RPC principal del sistema.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.trkwks", "TrkWks", "Seguimiento de vínculos distribuidos",
            "Mantiene los accesos directos a archivos que se mueven entre carpetas de la red.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.ndu", "Ndu", "Uso de datos de red",
            "Contabiliza el consumo de datos por aplicación. Si lo apagas, el Administrador de tareas deja de mostrar red por app.",
            RiskLevel.Advanced),

        Service("svc.nvtelemetry", "NvTelemetryContainer", "Telemetría de NVIDIA",
            "Envía datos de uso del driver a NVIDIA. No afecta al rendimiento en juegos.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.gupdate", "gupdate", "Actualizador de Google",
            "Mantiene Chrome al día en segundo plano. Al desactivarlo, actualiza Chrome tú mismo.",
            RiskLevel.Advanced),

        Service("svc.gupdatem", "gupdatem", "Actualizador de Google (secundario)",
            "Segundo servicio del actualizador de Chrome.",
            RiskLevel.Advanced),

        Service("svc.wisvc", "wisvc", "Programa Windows Insider",
            "Inscribe el equipo en las compilaciones de prueba de Windows.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.wpcmonsvc", "WpcMonSvc", "Control parental",
            "Supervisa y limita el uso del equipo por cuentas infantiles.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.remoteaccess", "RemoteAccess", "Enrutamiento y acceso remoto",
            "Convierte el equipo en enrutador o servidor VPN. Viene desactivado en la mayoría de equipos.",
            RiskLevel.Safe, defaultEnabled: true),

        Service("svc.printnotify", "PrintNotify", "Notificaciones de impresora",
            "Muestra avisos y extensiones de la impresora. Imprimir sigue funcionando sin él.",
            RiskLevel.Safe, defaultEnabled: true),

        // A Manual servicio arranca solo cuando algo lo pide: el escáner sigue funcionando y
        // Delivery Optimization deja de sembrar actualizaciones sin romper Windows Update.
        Service("svc.stisvc", "StiSvc", "Adquisición de imágenes (escáner)",
            "Pasa a inicio manual: el escáner sigue funcionando, pero el servicio no queda cargado siempre.",
            RiskLevel.Safe, defaultEnabled: true, startMode: "Manual"),

        Service("svc.dosvc", "DoSvc", "Optimización de distribución",
            "Pasa a inicio manual para que deje de compartir actualizaciones con otros equipos de internet. Windows Update sigue funcionando.",
            RiskLevel.Safe, defaultEnabled: true, startMode: "Manual"),
    };

    private static ServiceStateTweak Service(string id, string serviceName, string name, string description,
        RiskLevel risk, bool defaultEnabled = false, string startMode = "Disabled") => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Category = TweakCategory.Services,
        Risk = risk,
        DefaultEnabled = defaultEnabled,
        ServiceName = serviceName,
        DesiredStartMode = startMode,
    };
}
