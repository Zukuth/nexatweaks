using Microsoft.Win32;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record NagleInterfaceState(int? Ack, int? NoDelay);
public sealed record NagleState(Dictionary<string, NagleInterfaceState> PerInterface);

/// <summary>
/// Disables Nagle's algorithm (TcpAckFrequency=1, TCPNoDelay=1) on every network interface's
/// Tcpip parameters key - the classic low-latency network tweak. Applies to all interface GUIDs
/// under Tcpip\Parameters\Interfaces so it works regardless of which adapter is active.
/// </summary>
public sealed class NagleTweak : TweakBase
{
    private const string InterfacesKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

    public override bool IsApplied()
    {
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var interfaces = baseKey.OpenSubKey(InterfacesKey);
        if (interfaces is null) return false;

        foreach (var name in interfaces.GetSubKeyNames())
        {
            using var iface = interfaces.OpenSubKey(name);
            if (iface is null) continue;
            var ack = iface.GetValue("TcpAckFrequency");
            var noDelay = iface.GetValue("TCPNoDelay");
            if (ack is null || noDelay is null) return false;
            if (!ack.Equals(1) || !noDelay.Equals(1)) return false;
        }
        return true;
    }

    public override BackupEntry Apply()
    {
        var prior = new Dictionary<string, NagleInterfaceState>();
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var interfaces = baseKey.OpenSubKey(InterfacesKey, true);
        if (interfaces is null) return BackupEntry.Create(this, new NagleState(prior));

        foreach (var name in interfaces.GetSubKeyNames())
        {
            using var iface = interfaces.OpenSubKey(name, true);
            if (iface is null) continue;

            var ack = iface.GetValue("TcpAckFrequency") as int?;
            var noDelay = iface.GetValue("TCPNoDelay") as int?;
            prior[name] = new NagleInterfaceState(ack, noDelay);

            iface.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
            iface.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
        }

        return BackupEntry.Create(this, new NagleState(prior));
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<NagleState>();
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var interfaces = baseKey.OpenSubKey(InterfacesKey, true);
        if (interfaces is null) return;

        foreach (var (name, values) in state.PerInterface)
        {
            using var iface = interfaces.OpenSubKey(name, true);
            if (iface is null) continue;

            if (values.Ack is not null) iface.SetValue("TcpAckFrequency", values.Ack.Value, RegistryValueKind.DWord);
            else iface.DeleteValue("TcpAckFrequency", false);

            if (values.NoDelay is not null) iface.SetValue("TCPNoDelay", values.NoDelay.Value, RegistryValueKind.DWord);
            else iface.DeleteValue("TCPNoDelay", false);
        }
    }
}
