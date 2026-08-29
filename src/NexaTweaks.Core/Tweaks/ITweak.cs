using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public interface ITweak
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    TweakCategory Category { get; }
    RiskLevel Risk { get; }
    bool DefaultEnabled { get; }

    /// <summary>Whether the tweak is currently reversible via a snapshot (false for one-way cleanup actions).</summary>
    bool IsReversible { get; }

    /// <summary>Whether this tweak only fully takes effect after Windows restarts.</summary>
    bool RequiresRestart { get; }

    bool IsApplied();

    /// <summary>Captures prior state, applies the tweak, and returns a backup entry describing how to revert it.</summary>
    BackupEntry Apply();

    void Revert(BackupEntry entry);
}
