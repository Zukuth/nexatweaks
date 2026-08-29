using System.Diagnostics;
using System.Text.RegularExpressions;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public sealed record PowerPlanSettingState(string AcValue, string DcValue);

/// <summary>Changes a current power-plan setting while preserving the exact AC/DC values for rollback.</summary>
public sealed class PowerPlanSettingTweak : TweakBase
{
    public required string Subgroup { get; init; }
    public required string Setting { get; init; }
    public required string EnabledValue { get; init; }

    public override bool IsApplied()
    {
        var state = ReadState();
        return string.Equals(state.AcValue, EnabledValue, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(state.DcValue, EnabledValue, StringComparison.OrdinalIgnoreCase);
    }

    public override BackupEntry Apply()
    {
        var prior = ReadState();
        SetValues(EnabledValue, EnabledValue);
        return BackupEntry.Create(this, prior);
    }

    public override void Revert(BackupEntry entry)
    {
        var state = entry.As<PowerPlanSettingState>();
        SetValues(state.AcValue, state.DcValue);
    }

    public static string? ParseCurrentSettingIndex(string output, string powerSource)
    {
        var match = Regex.Match(output,
            $@"Current\s+{Regex.Escape(powerSource)}\s+Power\s+Setting\s+Index:\s*(0x[0-9a-fA-F]+)",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private PowerPlanSettingState ReadState()
    {
        var output = Run($"/q scheme_current {Subgroup} {Setting}");
        var ac = ParseCurrentSettingIndex(output, "AC");
        var dc = ParseCurrentSettingIndex(output, "DC");
        if (ac is null || dc is null)
            throw new InvalidOperationException($"No se pudo leer el estado de energía {Setting} del plan activo.");
        return new PowerPlanSettingState(ac, dc);
    }

    private void SetValues(string acValue, string dcValue)
    {
        Run($"/setacvalueindex scheme_current {Subgroup} {Setting} {acValue}");
        Run($"/setdcvalueindex scheme_current {Subgroup} {Setting} {dcValue}");
        Run("/setactive scheme_current");
    }

    private static string Run(string arguments)
    {
        var psi = new ProcessStartInfo("powercfg.exe", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException("No se pudo iniciar powercfg.exe.");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? output : error);
        return output;
    }
}
