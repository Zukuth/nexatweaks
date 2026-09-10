using System.Diagnostics;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record ShellState(string? Prior);

/// <summary>
/// Generic tweak driven by shell commands (powercfg, netsh, powershell), for state that has no
/// simple registry/service representation. CaptureState reads the current value, ApplyCommand
/// mutates it, RevertCommand rebuilds the original command from the captured value.
/// </summary>
public sealed class ShellCommandTweak : TweakBase
{
    public required Func<string?> CaptureState { get; init; }
    public required Func<string> ApplyCommand { get; init; }
    public required Func<string?, string> RevertCommand { get; init; }
    public Func<bool>? AppliedCheck { get; init; }

    public override bool IsApplied() => AppliedCheck?.Invoke() ?? false;

    public override BackupEntry Apply()
    {
        var prior = CaptureState();
        Exec(ApplyCommand());
        return BackupEntry.Create(this, new ShellState(prior));
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<ShellState>();
        Exec(RevertCommand(state.Prior));
    }

    public static string ExecCapture(string command)
    {
        var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var process = Process.Start(psi)!;
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return output;
    }

    private static void Exec(string command)
    {
        var psi = new ProcessStartInfo("cmd.exe", $"/c {command}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var process = Process.Start(psi)!;
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        // A non-zero exit code means the underlying command failed (bad syntax, access denied,
        // target not found, etc.). Without this check TweakEngine sees no exception and reports
        // the apply/revert as successful even though nothing actually changed.
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"El comando terminó con código {process.ExitCode}: {command}"
                + (string.IsNullOrWhiteSpace(stderr) ? "" : $"\n{stderr.Trim()}"));
        }
    }
}
