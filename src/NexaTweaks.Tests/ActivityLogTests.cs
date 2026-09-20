using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.Tests;

public class ActivityLogTests
{
    private static ActivityLog New() => new(capacity: 5);

    [Fact]
    public void Info_AddsEntryWithMessageAndLevel()
    {
        var log = New();

        log.Info("Analizando el sistema");

        var entry = Assert.Single(log.Entries);
        Assert.Equal("Analizando el sistema", entry.Message);
        Assert.Equal(ActivityLevel.Info, entry.Level);
    }

    [Fact]
    public void Levels_AreRecordedPerEntry()
    {
        var log = New();

        log.Ok("Aplicado");
        log.Warn("Requiere reinicio");
        log.Fail("No se pudo aplicar");

        Assert.Equal(
            new[] { ActivityLevel.Ok, ActivityLevel.Warn, ActivityLevel.Fail },
            log.Entries.Select(e => e.Level));
    }

    [Fact]
    public void Entries_AreKeptInChronologicalOrder()
    {
        var log = New();

        log.Info("primero");
        log.Info("segundo");

        Assert.Equal(new[] { "primero", "segundo" }, log.Entries.Select(e => e.Message));
    }

    [Fact]
    public void Entries_DropOldestWhenCapacityIsReached()
    {
        var log = New();

        for (var i = 1; i <= 8; i++) log.Info($"linea {i}");

        Assert.Equal(5, log.Entries.Count);
        Assert.Equal("linea 4", log.Entries[0].Message);
        Assert.Equal("linea 8", log.Entries[^1].Message);
    }

    [Fact]
    public void EntryAdded_FiresForEachEntry()
    {
        var log = New();
        var received = new List<ActivityEntry>();
        log.EntryAdded += received.Add;

        log.Ok("hecho");

        var entry = Assert.Single(received);
        Assert.Equal("hecho", entry.Message);
        Assert.Equal(ActivityLevel.Ok, entry.Level);
    }

    [Fact]
    public void Clear_EmptiesTheLogAndNotifies()
    {
        var log = New();
        log.Info("algo");
        var cleared = false;
        log.Cleared += () => cleared = true;

        log.Clear();

        Assert.Empty(log.Entries);
        Assert.True(cleared);
    }

    [Fact]
    public void ExportText_HasOneTimestampedLinePerEntry()
    {
        var log = New();
        log.Info("primero");
        log.Fail("segundo");

        var lines = log.ExportText().Split(Environment.NewLine);

        Assert.Equal(2, lines.Length);
        Assert.Matches(@"^\[\d{2}:\d{2}:\d{2}\] primero$", lines[0]);
        Assert.Matches(@"^\[\d{2}:\d{2}:\d{2}\] segundo$", lines[1]);
    }

    [Fact]
    public void Add_IsSafeFromManyThreadsAtOnce()
    {
        var log = new ActivityLog(capacity: 500);

        Parallel.For(0, 400, i => log.Info($"linea {i}"));

        Assert.Equal(400, log.Entries.Count);
    }

    [Fact]
    public void Instance_IsShared()
    {
        Assert.Same(ActivityLog.Instance, ActivityLog.Instance);
    }
}
