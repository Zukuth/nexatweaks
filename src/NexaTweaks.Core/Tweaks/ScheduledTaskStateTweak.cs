using System.Diagnostics;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record TaskState(string PriorStatus);

/// <summary>Enables/disables a Windows Scheduled Task via schtasks.exe.</summary>
public sealed class ScheduledTaskStateTweak : TweakBase
{
    public required string TaskPath { get; init; }

    public override bool IsApplied() =>
        string.Equals(GetStatus(), "Disabled", StringComparison.OrdinalIgnoreCase);

    public override BackupEntry Apply()
    {
        var prior = GetStatus() ?? "Ready";
        RunSchtasks($"/Change /TN \"{TaskPath}\" /Disable");
        return BackupEntry.Create(this, new TaskState(prior));
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<TaskState>();
        if (!state.PriorStatus.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
            RunSchtasks($"/Change /TN \"{TaskPath}\" /Enable");
    }

    private string? GetStatus()
    {
        var output = RunSchtasks($"/Query /TN \"{TaskPath}\" /FO LIST");
        var line = output
            .Split('\n')
            .FirstOrDefault(l => l.TrimStart().StartsWith("Status:", StringComparison.OrdinalIgnoreCase));
        return line?.Split(':', 2).ElementAtOrDefault(1)?.Trim();
    }

    private static string RunSchtasks(string args)
    {
        var psi = new ProcessStartInfo("schtasks.exe", args)
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
}
