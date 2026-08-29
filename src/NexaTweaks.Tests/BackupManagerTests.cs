using NexaTweaks.Core.Backup;
using Xunit;

namespace NexaTweaks.Tests;

public class BackupManagerTests
{
    [Fact]
    public void CreateSnapshot_PersistsAndReloads()
    {
        var dir = Path.Combine(Path.GetTempPath(), "NexaTweaksTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new BackupManager(dir);
            var entry = new BackupEntry("tweak.id", "Tweak Name", DateTimeOffset.Now, "{\"x\":1}");

            var snapshot = manager.CreateSnapshot("test snapshot", new[] { entry });

            var reloaded = new BackupManager(dir).LoadAll();
            Assert.Single(reloaded);
            Assert.Equal("test snapshot", reloaded[0].Label);
            Assert.Equal(snapshot.Id, reloaded[0].Id);
            Assert.Single(reloaded[0].Entries);
            Assert.Equal("tweak.id", reloaded[0].Entries[0].TweakId);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void Delete_RemovesSnapshotFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "NexaTweaksTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var manager = new BackupManager(dir);
            var snapshot = manager.CreateSnapshot("to delete", Array.Empty<BackupEntry>());

            manager.Delete(snapshot);

            Assert.Empty(manager.LoadAll());
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
