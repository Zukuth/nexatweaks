using Microsoft.Win32;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

/// <summary>
/// Captures a prior registry value in a JSON-serialization-safe shape. Numeric values (DWord/QWord)
/// go through NumericValue, strings through StringValue - avoids System.Text.Json boxing "object"
/// as JsonElement on deserialize, which RegistryKey.SetValue can't consume directly.
/// </summary>
public sealed record RegistryState(long? NumericValue, string? StringValue, bool Existed, RegistryValueKind Kind);

/// <summary>A tweak that sets a single registry value to an "enabled" value, capturing whatever was there before.</summary>
public sealed class RegistryTweak : TweakBase
{
    public required RegistryHive Hive { get; init; }
    public required string SubKey { get; init; }
    public required string ValueName { get; init; }
    public required object EnabledValue { get; init; }
    public RegistryValueKind ValueKind { get; init; } = RegistryValueKind.DWord;

    public override bool IsApplied()
    {
        using var key = OpenKey(false);
        var current = key?.GetValue(ValueName);
        return current is not null && current.Equals(EnabledValue);
    }

    public override BackupEntry Apply()
    {
        using var key = OpenKey(true)!;
        var prior = key.GetValue(ValueName);
        var state = CaptureState(prior);
        key.SetValue(ValueName, EnabledValue, ValueKind);
        return BackupEntry.Create(this, state);
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<RegistryState>();
        using var key = OpenKey(true)!;
        if (!state.Existed)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
        else if (state.StringValue is not null)
        {
            key.SetValue(ValueName, state.StringValue, RegistryValueKind.String);
        }
        else
        {
            object value = state.Kind == RegistryValueKind.QWord
                ? state.NumericValue!.Value
                : (int)state.NumericValue!.Value;
            key.SetValue(ValueName, value, state.Kind);
        }
    }

    private static RegistryState CaptureState(object? prior)
    {
        if (prior is null) return new RegistryState(null, null, false, RegistryValueKind.DWord);
        if (prior is string s) return new RegistryState(null, s, true, RegistryValueKind.String);
        return new RegistryState(Convert.ToInt64(prior), null, true,
            prior is long ? RegistryValueKind.QWord : RegistryValueKind.DWord);
    }

    private RegistryKey OpenKeyBase() => RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);

    private RegistryKey? OpenKey(bool writable)
    {
        using var baseKey = OpenKeyBase();
        return writable ? baseKey.CreateSubKey(SubKey, true) : baseKey.OpenSubKey(SubKey, false);
    }
}
