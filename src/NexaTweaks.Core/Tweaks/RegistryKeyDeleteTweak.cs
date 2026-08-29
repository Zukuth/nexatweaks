using Microsoft.Win32;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

/// <summary>A single registry value captured in a JSON-safe shape, with enough type info to write it back exactly as it was.</summary>
public sealed record RegistryValueSnapshot(string Kind, string? Data)
{
    private const string MultiStringSeparator = "";

    public static RegistryValueSnapshot From(object? raw, RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.DWord => new RegistryValueSnapshot("DWord", Convert.ToInt64(raw).ToString()),
        RegistryValueKind.QWord => new RegistryValueSnapshot("QWord", Convert.ToInt64(raw).ToString()),
        RegistryValueKind.MultiString => new RegistryValueSnapshot(
            "MultiString", string.Join(MultiStringSeparator, (string[]?)raw ?? Array.Empty<string>())),
        RegistryValueKind.Binary => new RegistryValueSnapshot("Binary", Convert.ToBase64String((byte[]?)raw ?? Array.Empty<byte>())),
        RegistryValueKind.ExpandString => new RegistryValueSnapshot("ExpandString", raw as string),
        _ => new RegistryValueSnapshot("String", raw?.ToString()),
    };

    public void WriteTo(RegistryKey key, string valueName)
    {
        switch (Kind)
        {
            case "DWord": key.SetValue(valueName, (int)long.Parse(Data!), RegistryValueKind.DWord); break;
            case "QWord": key.SetValue(valueName, long.Parse(Data!), RegistryValueKind.QWord); break;
            case "MultiString":
                key.SetValue(valueName, (Data ?? "").Split(new[] { MultiStringSeparator }, StringSplitOptions.None), RegistryValueKind.MultiString);
                break;
            case "Binary": key.SetValue(valueName, Convert.FromBase64String(Data!), RegistryValueKind.Binary); break;
            case "ExpandString": key.SetValue(valueName, Data ?? "", RegistryValueKind.ExpandString); break;
            default: key.SetValue(valueName, Data ?? "", RegistryValueKind.String); break;
        }
    }
}

public sealed record RegistryKeyState(Dictionary<string, RegistryValueSnapshot> Values, bool Existed);

/// <summary>
/// Deletes an entire registry subkey, capturing every value it held so Revert can rebuild it
/// exactly - used by the registry cleaner to remove orphaned entries without it being a one-way
/// operation like a real CCleaner-style registry cleaner usually is.
/// </summary>
public sealed class RegistryKeyDeleteTweak : TweakBase
{
    public required RegistryHive Hive { get; init; }
    public required string ParentSubKey { get; init; }
    public required string KeyName { get; init; }

    public override bool IsApplied()
    {
        using var baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
        using var parent = baseKey.OpenSubKey(ParentSubKey);
        using var target = parent?.OpenSubKey(KeyName);
        return target is null;
    }

    public override BackupEntry Apply()
    {
        using var baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
        using var parent = baseKey.OpenSubKey(ParentSubKey, true);
        if (parent is null) return BackupEntry.Create(this, new RegistryKeyState(new(), false));

        var values = new Dictionary<string, RegistryValueSnapshot>();
        var existed = false;

        using (var target = parent.OpenSubKey(KeyName))
        {
            if (target is not null)
            {
                existed = true;
                foreach (var valueName in target.GetValueNames())
                {
                    var raw = target.GetValue(valueName);
                    var kind = target.GetValueKind(valueName);
                    values[valueName] = RegistryValueSnapshot.From(raw, kind);
                }
            }
        }

        parent.DeleteSubKeyTree(KeyName, throwOnMissingSubKey: false);
        return BackupEntry.Create(this, new RegistryKeyState(values, existed));
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<RegistryKeyState>();
        if (!state.Existed) return;

        using var baseKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
        using var parent = baseKey.OpenSubKey(ParentSubKey, true)!;
        using var key = parent.CreateSubKey(KeyName, true);
        foreach (var (name, snapshot) in state.Values)
            snapshot.WriteTo(key, name);
    }
}
