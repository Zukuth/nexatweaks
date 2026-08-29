using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

/// <summary>
/// One-way cleanup action (deleting temp/cache files). Not reversible - deleted temp files
/// can't be restored, so Revert is a no-op and IsReversible is false so the UI can say so.
/// </summary>
public sealed class FileCleanupTweak : TweakBase
{
    public required IReadOnlyList<string> TargetDirectories { get; init; }

    public override bool IsReversible => false;

    public long LastBytesFreed { get; private set; }

    public override bool IsApplied() => false;

    public override BackupEntry Apply()
    {
        long freed = 0;
        foreach (var dir in TargetDirectories)
        {
            if (!Directory.Exists(dir)) continue;
            freed += DeleteContents(dir);
        }
        LastBytesFreed = freed;
        return BackupEntry.CreateEmpty(this);
    }

    public override void Revert(BackupEntry entry)
    {
        // no-op: deleted temp/cache files cannot be recovered
    }

    private static long DeleteContents(string directory)
    {
        long freed = 0;
        foreach (var file in SafeEnumerateFiles(directory))
        {
            try
            {
                var info = new FileInfo(file);
                var size = info.Exists ? info.Length : 0;
                info.Delete();
                freed += size;
            }
            catch
            {
                // file in use / access denied - skip it
            }
        }

        foreach (var dir in SafeEnumerateDirectories(directory))
        {
            try
            {
                if (Directory.GetFileSystemEntries(dir).Length == 0)
                    Directory.Delete(dir);
            }
            catch
            {
                // not empty or in use - skip it
            }
        }

        return freed;
    }

    private static IEnumerable<string> SafeEnumerateFiles(string dir)
    {
        try { return Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories); }
        catch { return Enumerable.Empty<string>(); }
    }

    private static IEnumerable<string> SafeEnumerateDirectories(string dir)
    {
        try { return Directory.EnumerateDirectories(dir, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length); }
        catch { return Enumerable.Empty<string>(); }
    }
}
