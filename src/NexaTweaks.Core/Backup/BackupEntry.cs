using System.Text.Json;

namespace NexaTweaks.Core.Backup;

public sealed record BackupEntry(string TweakId, string TweakName, DateTimeOffset Timestamp, string? StateJson)
{
    public static BackupEntry Create<T>(Tweaks.ITweak tweak, T state) =>
        new(tweak.Id, tweak.Name, DateTimeOffset.Now, JsonSerializer.Serialize(state));

    public static BackupEntry CreateEmpty(Tweaks.ITweak tweak) =>
        new(tweak.Id, tweak.Name, DateTimeOffset.Now, null);

    public T As<T>() => JsonSerializer.Deserialize<T>(StateJson!)!;
}
