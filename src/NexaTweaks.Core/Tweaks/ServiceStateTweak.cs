using System.Management;
using System.ServiceProcess;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record ServiceState(string PriorStartMode);

/// <summary>Changes a Windows service's start mode (Boot/System/Automatic/Manual/Disabled) via WMI.</summary>
public sealed class ServiceStateTweak : TweakBase
{
    public required string ServiceName { get; init; }
    public required string DesiredStartMode { get; init; }
    public bool StopServiceWhenDisabling { get; init; } = true;

    public override bool IsAvailable() => GetStartMode() is not null;

    public override bool IsApplied()
    {
        var mode = GetStartMode();
        return string.Equals(mode, DesiredStartMode, StringComparison.OrdinalIgnoreCase);
    }

    public override BackupEntry Apply()
    {
        var prior = GetStartMode() ?? "Auto";
        SetStartMode(DesiredStartMode);

        if (StopServiceWhenDisabling && DesiredStartMode.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            TryStopService();

        return BackupEntry.Create(this, new ServiceState(prior));
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<ServiceState>();
        SetStartMode(state.PriorStartMode);

        if (state.PriorStartMode.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            TryStartService();
    }

    private string? GetStartMode() => ServiceStartModes.Get(ServiceName);

    private void SetStartMode(string mode)
    {
        using var searcher = new ManagementObjectSearcher(
            $"SELECT * FROM Win32_Service WHERE Name='{Escape(ServiceName)}'");
        var found = false;

        foreach (ManagementObject mo in searcher.Get())
        {
            found = true;
            // ChangeStartMode returns a uint32 status code, not a bool - Windows can (and does,
            // for hardened services like wuauserv on recent builds) accept the WMI call but
            // silently refuse the change. Ignoring this return value is how a failed tweak used
            // to get reported back as a success.
            var raw = mo.InvokeMethod("ChangeStartMode", new object[] { mode });
            var returnValue = raw is null ? 1u : Convert.ToUInt32(raw);
            if (returnValue != 0)
            {
                throw new InvalidOperationException(
                    $"Windows rechazó el cambio en el servicio '{ServiceName}' (código {returnValue}). " +
                    "Algunos servicios protegidos (como Windows Update) ya no permiten deshabilitarse así en versiones recientes de Windows.");
            }
        }

        if (!found)
            throw new InvalidOperationException($"No se encontró el servicio '{ServiceName}' en este equipo.");

        ServiceStartModes.Invalidate();
    }

    private void TryStopService()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            if (sc.Status != ServiceControllerStatus.Stopped)
            {
                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
            }
        }
        catch
        {
            // best effort - some services refuse to stop, that's fine, start mode still changes
        }
    }

    private void TryStartService()
    {
        try
        {
            using var sc = new ServiceController(ServiceName);
            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
            }
        }
        catch
        {
            // best effort
        }
    }

    private static string Escape(string value) => value.Replace("'", "''");
}
