using NexaTweaks.Core;
using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Engine;
using NexaTweaks.Core.Tweaks;
using Xunit;

namespace NexaTweaks.Tests;

public class TweakEngineTests
{
    private sealed class FakeTweak : TweakBase
    {
        public bool Applied { get; private set; }
        public bool ThrowOnApply { get; init; }

        public override bool IsApplied() => Applied;

        public override BackupEntry Apply()
        {
            if (ThrowOnApply) throw new InvalidOperationException("boom");
            Applied = true;
            return BackupEntry.Create(this, new { WasApplied = false });
        }

        public override void Revert(BackupEntry entry) => Applied = false;
    }

    private static FakeTweak MakeFake(string id, bool throwOnApply = false) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        Category = TweakCategory.Windows,
        ThrowOnApply = throwOnApply,
    };

    [Fact]
    public void ApplyMany_CreatesSnapshotWithOnlyReversibleEntries()
    {
        var dir = Path.Combine(Path.GetTempPath(), "NexaTweaksTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new BackupManager(dir);
            var engine = new TweakEngine(manager);
            var tweak = MakeFake("a");

            var (snapshot, results) = engine.ApplyMany(new[] { tweak }, "label");

            Assert.True(results[0].Success);
            Assert.True(tweak.Applied);
            Assert.Single(snapshot.Entries);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void ApplyMany_ContinuesAfterFailure_AndReportsError()
    {
        var dir = Path.Combine(Path.GetTempPath(), "NexaTweaksTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new BackupManager(dir);
            var engine = new TweakEngine(manager);
            var failing = MakeFake("fail", throwOnApply: true);
            var ok = MakeFake("ok");

            var (_, results) = engine.ApplyMany(new ITweak[] { failing, ok }, "label");

            Assert.False(results[0].Success);
            Assert.NotNull(results[0].Error);
            Assert.True(results[1].Success);
            Assert.True(ok.Applied);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void RestoreSnapshot_RevertsEachKnownTweak_AndDeletesSnapshot()
    {
        var dir = Path.Combine(Path.GetTempPath(), "NexaTweaksTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new BackupManager(dir);
            var engine = new TweakEngine(manager);
            var tweak = MakeFake("a");

            var (snapshot, _) = engine.ApplyMany(new[] { tweak }, "label");
            Assert.True(tweak.Applied);

            var results = engine.RestoreSnapshot(snapshot, new Dictionary<string, ITweak> { ["a"] = tweak });

            Assert.True(results[0].Success);
            Assert.False(tweak.Applied);
            Assert.Empty(manager.LoadAll());
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
