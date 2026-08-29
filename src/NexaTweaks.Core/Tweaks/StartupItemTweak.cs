using Microsoft.Win32;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record StartupItemState(string Command);

/// <summary>
/// Disables a Run-key startup entry by removing its value (backing up the command so it can be
/// restored). Built dynamically at runtime from whatever is actually present in the registry,
/// rather than a fixed catalog entry.
/// </summary>
public sealed class StartupItemTweak : TweakBase
{
    public required RegistryHive Hive { get; init; }
    public required string SubKey { get; init; }
    public required string ValueName { get; init; }

    public override bool IsApplied()
    {
        using var key = OpenKey(false);
        return key?.GetValue(ValueName) is null;
    }

    public override BackupEntry Apply()
    {
        using var key = OpenKey(true)!;
        var command = key.GetValue(ValueName) as string ?? "";
        key.DeleteValue(ValueName, throwOnMissingValue: false);
        return BackupEntry.Create(this, new StartupItemState(command));
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<StartupItemState>();
        using var key = OpenKey(true)!;
        key.SetValue(ValueName, state.Command, RegistryValueKind.String);
    }

    private RegistryKey? OpenKey(bool writable)
    {
        using var baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
        return writable ? baseKey.CreateSubKey(SubKey, true) : baseKey.OpenSubKey(SubKey, false);
    }
}
