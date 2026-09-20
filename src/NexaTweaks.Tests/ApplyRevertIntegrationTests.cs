using Microsoft.Win32;
using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Engine;
using NexaTweaks.Core.Tweaks;
using Xunit.Abstractions;

namespace NexaTweaks.Tests;

/// <summary>
/// Applies real catalog tweaks against this machine's registry and puts everything back.
/// Only runs with NEXA_INTEGRATION=1 (see <see cref="IntegrationFactAttribute"/>) because it
/// writes to HKCU; every test restores the previous value even if the assertions fail.
/// </summary>
public class ApplyRevertIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public ApplyRevertIntegrationTests(ITestOutputHelper output) => _output = output;

    private static TweakEngine NewEngine(string backupRoot) => new(new BackupManager(backupRoot));

    private static string TempBackupRoot() =>
        Path.Combine(Path.GetTempPath(), "NexaTweaks.IntegrationTests", Guid.NewGuid().ToString("N"));

    [IntegrationFact]
    public void DwordTweak_AppliesAndRevertsLeavingTheRegistryAsItWas()
    {
        var tweak = (RegistryTweak)TweakCatalog.Interface.Single(t => t.Id == "ui.explorer.extensions");
        var before = ReadValue(tweak);
        var root = TempBackupRoot();
        var engine = NewEngine(root);

        try
        {
            var (result, entry) = engine.ApplyOneWithEntry(tweak, "Prueba de integración");

            Assert.True(result.Success, result.Error);
            Assert.NotNull(entry);
            Assert.Equal(0, Convert.ToInt32(ReadValue(tweak)));
            Assert.True(tweak.IsApplied());
            _output.WriteLine($"Aplicado: HideFileExt = {ReadValue(tweak)} (antes: {before ?? "(no existía)"})");

            var revert = engine.RevertOne(tweak, entry!);

            Assert.True(revert.Success, revert.Error);
            Assert.Equal(before?.ToString(), ReadValue(tweak)?.ToString());
            _output.WriteLine($"Revertido: HideFileExt = {ReadValue(tweak) ?? "(borrado)"}");
        }
        finally
        {
            Restore(tweak, before);
            Directory.Delete(root, recursive: true);
        }
    }

    [IntegrationFact]
    public void StringTweak_IsWrittenAsRegSz_SoWindowsActuallyReadsIt()
    {
        var tweak = (RegistryTweak)TweakCatalog.Windows.Single(t => t.Id == "win.waittokillapp");
        var before = ReadValue(tweak);
        var root = TempBackupRoot();
        var engine = NewEngine(root);

        try
        {
            var (result, entry) = engine.ApplyOneWithEntry(tweak, "Prueba de integración");

            Assert.True(result.Success, result.Error);
            Assert.Equal("2000", ReadValue(tweak));
            Assert.Equal(RegistryValueKind.String, ReadKind(tweak));
            _output.WriteLine($"Aplicado: WaitToKillAppTimeout = {ReadValue(tweak)} ({ReadKind(tweak)})");

            var revert = engine.RevertOne(tweak, entry!);

            Assert.True(revert.Success, revert.Error);
            Assert.Equal(before?.ToString(), ReadValue(tweak)?.ToString());
        }
        finally
        {
            Restore(tweak, before);
            Directory.Delete(root, recursive: true);
        }
    }

    [IntegrationFact]
    public void Apply_WritesABackupSnapshotThatCanBeRestoredById()
    {
        var tweak = (RegistryTweak)TweakCatalog.Interface.Single(t => t.Id == "ui.explorer.hidden");
        var before = ReadValue(tweak);
        var root = TempBackupRoot();
        var backups = new BackupManager(root);
        var engine = new TweakEngine(backups);

        try
        {
            engine.ApplyMany(new ITweak[] { tweak }, "Prueba de integración");

            var snapshot = Assert.Single(backups.LoadAll());
            var saved = Assert.Single(snapshot.Entries);
            Assert.Equal(tweak.Id, saved.TweakId);

            var results = engine.RestoreSnapshot(snapshot, new Dictionary<string, ITweak> { [tweak.Id] = tweak });

            Assert.All(results, r => Assert.True(r.Success, r.Error));
            Assert.Equal(before?.ToString(), ReadValue(tweak)?.ToString());
            _output.WriteLine($"Snapshot restaurado; Hidden = {ReadValue(tweak) ?? "(borrado)"}");
        }
        finally
        {
            Restore(tweak, before);
            Directory.Delete(root, recursive: true);
        }
    }

    private static object? ReadValue(RegistryTweak tweak)
    {
        using var key = RegistryKey.OpenBaseKey(tweak.Hive, RegistryView.Registry64).OpenSubKey(tweak.SubKey);
        return key?.GetValue(tweak.ValueName);
    }

    private static RegistryValueKind? ReadKind(RegistryTweak tweak)
    {
        using var key = RegistryKey.OpenBaseKey(tweak.Hive, RegistryView.Registry64).OpenSubKey(tweak.SubKey);
        return key?.GetValue(tweak.ValueName) is null ? null : key.GetValueKind(tweak.ValueName);
    }

    private static void Restore(RegistryTweak tweak, object? original)
    {
        using var key = RegistryKey.OpenBaseKey(tweak.Hive, RegistryView.Registry64).CreateSubKey(tweak.SubKey, true);
        if (original is null) key.DeleteValue(tweak.ValueName, throwOnMissingValue: false);
        else key.SetValue(tweak.ValueName, original, tweak.ValueKind);
    }
}
