using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record AppxState(bool WasInstalled);

/// <summary>
/// Uninstalls a preinstalled Store app for the current user and removes its provisioned copy, so
/// Windows doesn't put it back on the next new user profile.
/// One-way on purpose: reinstalling means downloading it again from the Store, which the app
/// cannot promise, so it declares itself non-reversible instead of faking an undo.
/// </summary>
public sealed class AppxPackageTweak : TweakBase
{
    public required string PackageName { get; init; }

    public override bool IsReversible => false;

    public override bool IsApplied() => !IsInstalled();

    public override BackupEntry Apply()
    {
        var wasInstalled = IsInstalled();
        if (wasInstalled) Run(BuildRemoveCommand(PackageName));
        return BackupEntry.Create(this, new AppxState(wasInstalled));
    }

    public override void Revert(BackupEntry entry) => throw new NotSupportedException(
        $"«{Name}» se desinstaló. Para recuperarla, búscala en Microsoft Store.");

    /// <summary>Removes it for this user and un-provisions it for future users, in one call.</summary>
    public static string BuildRemoveCommand(string packageName) =>
        "powershell -NoProfile -ExecutionPolicy Bypass -Command \"" +
        $"Get-AppxPackage -Name '{packageName}' | Remove-AppxPackage -ErrorAction SilentlyContinue; " +
        $"Get-AppxProvisionedPackage -Online | Where-Object {{ $_.DisplayName -eq '{packageName}' }} | " +
        "Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue | Out-Null\"";

    public static string BuildIsInstalledCommand(string packageName) =>
        "powershell -NoProfile -ExecutionPolicy Bypass -Command \"" +
        $"if (Get-AppxPackage -Name '{packageName}') {{ 'yes' }} else {{ 'no' }}\"";

    private bool IsInstalled()
    {
        try
        {
            return ShellCommandTweak.ExecCapture(BuildIsInstalledCommand(PackageName))
                .Contains("yes", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static void Run(string command)
    {
        // Remove-AppxPackage reports failures through its own error stream and still exits 0, so
        // success is verified by checking the package afterwards instead of by the exit code.
        ShellCommandTweak.ExecCapture(command);
    }
}
