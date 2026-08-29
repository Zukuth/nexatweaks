using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Booster;
using NexaTweaks.Core.Engine;
using NexaTweaks.Core.Monitoring;

namespace NexaTweaks.App.Services;

/// <summary>Simple shared-instance holder for the app's long-lived services (lightweight substitute for a DI container).</summary>
public static class AppServices
{
    public static BackupManager BackupManager { get; } = new();
    public static TweakEngine Engine { get; } = new(BackupManager);
    public static SystemStatsService Stats { get; } = new();
    public static PingService Ping { get; } = new();
    public static ProcessBoosterService Booster { get; } = new();
    public static GameProfileStore GameProfiles { get; } = new();
}
