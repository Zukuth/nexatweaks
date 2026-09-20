using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Stability;

public enum StabilitySeverity
{
    Info,
    Low,
    Medium,
    High,
}

/// <summary>
/// One problem the stability analysis found. <see cref="Fix"/> is set only when the app can
/// correct it safely through the tweak engine; hardware and firmware problems carry just a
/// <see cref="Recommendation"/>.
/// </summary>
public sealed record StabilityFinding(
    string CheckId,
    string Title,
    StabilitySeverity Severity,
    IReadOnlyList<string> Evidence,
    string Recommendation,
    ITweak? Fix = null);
