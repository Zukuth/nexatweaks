using Microsoft.Win32;
using NexaTweaks.Core;
using NexaTweaks.Core.Tweaks;
using Xunit;

namespace NexaTweaks.Tests;

public class RegistryTweakTests
{
    private const string TestSubKey = @"Software\NexaTweaks.Tests";

    private static RegistryTweak MakeTweak(string valueName, object enabledValue) => new()
    {
        Id = "test.tweak",
        Name = "Test tweak",
        Description = "",
        Category = TweakCategory.Windows,
        Hive = RegistryHive.CurrentUser,
        SubKey = TestSubKey,
        ValueName = valueName,
        EnabledValue = enabledValue,
    };

    [Fact]
    public void Apply_SetsValue_AndReportsApplied()
    {
        var tweak = MakeTweak("DidNotExistBefore", 1);
        try
        {
            Assert.False(tweak.IsApplied());
            tweak.Apply();
            Assert.True(tweak.IsApplied());
        }
        finally
        {
            CleanupValue("DidNotExistBefore");
        }
    }

    [Fact]
    public void Apply_ThenRevert_RestoresMissingValue()
    {
        var tweak = MakeTweak("RoundTripMissing", 1);
        try
        {
            var entry = tweak.Apply();
            Assert.True(tweak.IsApplied());

            tweak.Revert(entry);

            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            using var key = baseKey.OpenSubKey(TestSubKey);
            Assert.Null(key?.GetValue("RoundTripMissing"));
        }
        finally
        {
            CleanupValue("RoundTripMissing");
        }
    }

    [Fact]
    public void Apply_ThenRevert_RestoresPriorValue()
    {
        using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
        using (var key = baseKey.CreateSubKey(TestSubKey, true))
        {
            key.SetValue("RoundTripExisting", 42, RegistryValueKind.DWord);
        }

        var tweak = MakeTweak("RoundTripExisting", 1);
        try
        {
            var entry = tweak.Apply();

            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var key = baseKey.OpenSubKey(TestSubKey))
            {
                Assert.Equal(1, key!.GetValue("RoundTripExisting"));
            }

            tweak.Revert(entry);

            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            using (var key = baseKey.OpenSubKey(TestSubKey))
            {
                Assert.Equal(42, key!.GetValue("RoundTripExisting"));
            }
        }
        finally
        {
            CleanupValue("RoundTripExisting");
        }
    }

    private static void CleanupValue(string valueName)
    {
        using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        using var key = baseKey.OpenSubKey(TestSubKey, true);
        key?.DeleteValue(valueName, throwOnMissingValue: false);
    }
}
