using Microsoft.Win32;
using NexaTweaks.Core.Settings;

namespace NexaTweaks.Tests;

public class AppPreferencesTests : IDisposable
{
    // Deliberately outside Software\NexaTweaks.Tests: RegistryTweakTests owns that key and
    // xUnit runs test classes in parallel, so a shared root would delete their values mid-test.
    private const string TestSubKey = @"Software\NexaTweaks.PreferencesTests";

    private static AppPreferences New() => new(TestSubKey);

    [Fact]
    public void CreateRestorePoint_DefaultsToTrue_SoChangesAreUndoableOutOfTheBox()
    {
        Assert.True(New().CreateRestorePoint);
    }

    [Fact]
    public void CreateRestorePoint_RoundTripsFalse()
    {
        var prefs = New();

        prefs.CreateRestorePoint = false;

        Assert.False(New().CreateRestorePoint);
    }

    [Fact]
    public void CreateRestorePoint_RoundTripsTrue()
    {
        var prefs = New();
        prefs.CreateRestorePoint = false;

        prefs.CreateRestorePoint = true;

        Assert.True(New().CreateRestorePoint);
    }

    public void Dispose() =>
        Registry.CurrentUser.DeleteSubKeyTree(TestSubKey, throwOnMissingSubKey: false);
}
