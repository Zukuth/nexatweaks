using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Core.Engine;

public sealed record TweakResult(ITweak Tweak, bool Success, string? Error);

/// <summary>
/// Orchestrates applying/reverting tweaks: every Apply is snapshotted to a BackupSnapshot
/// before anything is mutated, so a category (or the whole run) can always be restored.
/// </summary>
public sealed class TweakEngine
{
    private readonly BackupManager _backupManager;

    public TweakEngine(BackupManager backupManager) => _backupManager = backupManager;

    public (BackupSnapshot Snapshot, IReadOnlyList<TweakResult> Results) ApplyMany(
        IEnumerable<ITweak> tweaks, string snapshotLabel)
    {
        var entries = new List<BackupEntry>();
        var results = new List<TweakResult>();

        foreach (var tweak in tweaks)
        {
            try
            {
                var entry = tweak.Apply();
                if (tweak.IsReversible) entries.Add(entry);
                results.Add(new TweakResult(tweak, true, null));
            }
            catch (Exception ex)
            {
                results.Add(new TweakResult(tweak, false, ex.Message));
            }
        }

        var snapshot = _backupManager.CreateSnapshot(snapshotLabel, entries);
        return (snapshot, results);
    }

    public TweakResult ApplyOne(ITweak tweak, string snapshotLabel)
    {
        var (_, results) = ApplyMany(new[] { tweak }, snapshotLabel);
        return results[0];
    }

    /// <summary>
    /// Same as <see cref="ApplyOne"/> but also hands back the exact backup entry it just wrote,
    /// so callers can remember it directly instead of re-scanning every snapshot on disk
    /// afterward (which gets slower the longer the app has been used).
    /// </summary>
    public (TweakResult Result, BackupEntry? Entry) ApplyOneWithEntry(ITweak tweak, string snapshotLabel)
    {
        try
        {
            var entry = tweak.Apply();
            if (tweak.IsReversible) _backupManager.CreateSnapshot(snapshotLabel, new[] { entry });
            return (new TweakResult(tweak, true, null), tweak.IsReversible ? entry : null);
        }
        catch (Exception ex)
        {
            return (new TweakResult(tweak, false, ex.Message), null);
        }
    }

    /// <summary>Reverts a single tweak using a previously captured entry, without touching any snapshot file.</summary>
    public TweakResult RevertOne(ITweak tweak, BackupEntry entry)
    {
        try
        {
            tweak.Revert(entry);
            return new TweakResult(tweak, true, null);
        }
        catch (Exception ex)
        {
            return new TweakResult(tweak, false, ex.Message);
        }
    }

    /// <summary>Builds a map of tweak id -> most recent backup entry across every stored snapshot.</summary>
    public Dictionary<string, BackupEntry> LoadLatestEntriesByTweakId()
    {
        var map = new Dictionary<string, BackupEntry>();
        foreach (var snapshot in _backupManager.LoadAll().OrderBy(s => s.Timestamp))
        {
            foreach (var entry in snapshot.Entries)
                map[entry.TweakId] = entry;
        }
        return map;
    }

    public IReadOnlyList<TweakResult> RestoreSnapshot(
        BackupSnapshot snapshot, IReadOnlyDictionary<string, ITweak> tweaksById)
    {
        var results = new List<TweakResult>();

        foreach (var entry in snapshot.Entries)
        {
            if (!tweaksById.TryGetValue(entry.TweakId, out var tweak))
            {
                results.Add(new TweakResult(
                    new UnknownTweakPlaceholder(entry.TweakId, entry.TweakName), false,
                    "Tweak desconocido (puede que ya no exista en el catálogo)."));
                continue;
            }

            try
            {
                tweak.Revert(entry);
                results.Add(new TweakResult(tweak, true, null));
            }
            catch (Exception ex)
            {
                results.Add(new TweakResult(tweak, false, ex.Message));
            }
        }

        _backupManager.Delete(snapshot);
        return results;
    }

    private sealed class UnknownTweakPlaceholder : ITweak
    {
        public UnknownTweakPlaceholder(string id, string name) { Id = id; Name = name; }
        public string Id { get; }
        public string Name { get; }
        public string Description => "";
        public TweakCategory Category => TweakCategory.Windows;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool DefaultEnabled => false;
        public bool IsReversible => false;
        public bool RequiresRestart => false;
        public bool IsApplied() => false;
        public BackupEntry Apply() => throw new NotSupportedException();
        public void Revert(BackupEntry entry) { }
    }
}
