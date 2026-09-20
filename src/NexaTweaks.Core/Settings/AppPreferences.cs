using Microsoft.Win32;

namespace NexaTweaks.Core.Settings;

/// <summary>User preferences of the app itself (not system tweaks), stored per user in HKCU.</summary>
public sealed class AppPreferences
{
    public const string DefaultSubKey = @"Software\NexaTweaks";

    public static AppPreferences Instance { get; } = new();

    private readonly string _subKey;

    public AppPreferences(string subKey = DefaultSubKey) => _subKey = subKey;

    /// <summary>Create a Windows restore point before applying changes. On by default: an
    /// unrecoverable machine is far worse than the seconds the restore point costs.</summary>
    public bool CreateRestorePoint
    {
        get => GetBool(nameof(CreateRestorePoint), defaultValue: true);
        set => SetBool(nameof(CreateRestorePoint), value);
    }

    private bool GetBool(string name, bool defaultValue)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(_subKey, writable: false);
            return key?.GetValue(name) is int value ? value != 0 : defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private void SetBool(string name, bool value)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(_subKey, writable: true);
            key.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
        }
        catch
        {
            // preferences are a convenience - never break the app over them
        }
    }
}
