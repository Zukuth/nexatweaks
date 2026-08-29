using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

public abstract class TweakBase : ITweak
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required TweakCategory Category { get; init; }
    public RiskLevel Risk { get; init; } = RiskLevel.Safe;
    public bool DefaultEnabled { get; init; }
    public virtual bool IsReversible { get; init; } = true;
    public bool RequiresRestart { get; init; }

    public abstract bool IsApplied();
    public abstract BackupEntry Apply();
    public abstract void Revert(BackupEntry entry);
}
