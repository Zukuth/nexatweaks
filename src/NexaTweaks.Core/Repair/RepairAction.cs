using System.Diagnostics;

namespace NexaTweaks.Core.Repair;

public sealed record RepairActionResult(bool Success, string Output);

/// <summary>
/// A one-shot Windows repair/reset command (SFC, DISM, network stack reset, etc.) - unlike a
/// Tweak, there's no "state" to snapshot and revert, these just run an official Windows repair
/// tool and report what happened.
/// </summary>
public sealed class RepairAction
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required RiskLevel Risk { get; init; }
    public required string Executable { get; init; }
    public required string Arguments { get; init; }
    public bool RequiresConfirmationInput { get; init; }
    public string? EstimatedDuration { get; init; }
    public bool RequiresRestart { get; init; }

    public RepairActionResult Run()
    {
        try
        {
            var psi = new ProcessStartInfo(Executable, Arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = RequiresConfirmationInput,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi)!;

            if (RequiresConfirmationInput)
            {
                process.StandardInput.WriteLine("Y");
                process.StandardInput.Close();
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            var text = string.IsNullOrWhiteSpace(output) ? error : output;
            return new RepairActionResult(process.ExitCode == 0, Tail(text, 25));
        }
        catch (Exception ex)
        {
            return new RepairActionResult(false, ex.Message);
        }
    }

    private static string Tail(string text, int lines) =>
        string.Join('\n', text.Replace("\r", "").Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .TakeLast(lines));
}
