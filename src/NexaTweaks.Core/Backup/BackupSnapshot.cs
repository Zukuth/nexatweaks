namespace NexaTweaks.Core.Backup;

public sealed class BackupSnapshot
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public string Label { get; init; } = "";
    public List<BackupEntry> Entries { get; init; } = new();
}
