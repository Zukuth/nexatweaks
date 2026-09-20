using NexaTweaks.Core;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Presets;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

public class DebloatCatalogTests
{
    [Fact]
    public void Debloat_ListsTheUsualPreinstalledApps()
    {
        var ids = TweakCatalog.Debloat.Select(t => t.Id).ToList();

        Assert.All(new[]
        {
            "app.3dbuilder", "app.mixedreality", "app.solitaire", "app.gethelp", "app.feedbackhub",
            "app.maps", "app.news", "app.weather", "app.people", "app.skype", "app.clipchamp",
            "app.paint3d", "app.soundrecorder", "app.yourphone", "app.xboxapp", "app.xboxtcui",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Debloat_NeverRemovesAppsWindowsOrTheUserDependsOn()
    {
        var packages = TweakCatalog.Debloat.OfType<AppxPackageTweak>().Select(t => t.PackageName).ToList();

        Assert.All(new[]
        {
            "Microsoft.WindowsStore",        // sin Tienda no se reinstala nada
            "Microsoft.SecHealthUI",         // interfaz de Windows Defender
            "Microsoft.DesktopAppInstaller", // winget
            "Microsoft.VCLibs",              // dependencia de otras apps
            "Microsoft.UI.Xaml",             // dependencia de otras apps
            "Microsoft.WindowsTerminal",
            "Microsoft.GamingServices",      // necesaria para Game Pass
        }, blocked => Assert.DoesNotContain(packages, p => p.Contains(blocked, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Debloat_IsOneWay_AndNeverEnabledByDefault()
    {
        Assert.All(TweakCatalog.Debloat, t =>
        {
            Assert.False(t.IsReversible, $"{t.Id} dice ser reversible: desinstalar una app no lo es");
            Assert.False(t.DefaultEnabled, $"{t.Id} viene activado por defecto");
            Assert.Equal(TweakCategory.Debloat, t.Category);
        });
    }

    [Fact]
    public void RemoveCommand_TargetsTheUserAndTheProvisionedCopy()
    {
        var command = AppxPackageTweak.BuildRemoveCommand("Microsoft.XboxApp");

        Assert.Contains("Get-AppxPackage", command);
        Assert.Contains("Microsoft.XboxApp", command);
        Assert.Contains("Remove-AppxPackage", command);
        // Sin quitar la copia aprovisionada, Windows la reinstala en el próximo usuario nuevo.
        Assert.Contains("Get-AppxProvisionedPackage", command);
    }

    [Fact]
    public void PackageNames_AreExactNames_NeverWildcardsThatCouldMatchTooMuch()
    {
        Assert.All(TweakCatalog.Debloat.OfType<AppxPackageTweak>(), t =>
        {
            Assert.DoesNotContain("*", t.PackageName);
            Assert.Contains(".", t.PackageName);
            Assert.DoesNotContain(" ", t.PackageName);
        });
    }
}

public class PresetCatalogTests
{
    private static Preset Get(string id) => PresetCatalog.All.Single(p => p.Id == id);

    [Fact]
    public void Presets_AreTheThreeLevels()
    {
        Assert.Equal(new[] { "preset.seguro", "preset.medio", "preset.extremo" },
            PresetCatalog.All.Select(p => p.Id));
        Assert.All(PresetCatalog.All, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Name));
            Assert.False(string.IsNullOrWhiteSpace(p.Description));
            Assert.NotEmpty(p.Tweaks);
        });
    }

    [Fact]
    public void EachLevel_ContainsThePreviousOne()
    {
        var seguro = Get("preset.seguro").Tweaks.Select(t => t.Id).ToHashSet();
        var medio = Get("preset.medio").Tweaks.Select(t => t.Id).ToHashSet();
        var extremo = Get("preset.extremo").Tweaks.Select(t => t.Id).ToHashSet();

        Assert.ProperSubset(medio, seguro);
        Assert.ProperSubset(extremo, medio);
    }

    [Fact]
    public void NoPreset_EverIncludesRiskyOrIrreversibleChanges()
    {
        Assert.All(PresetCatalog.All, preset => Assert.All(preset.Tweaks, t =>
        {
            Assert.NotEqual(RiskLevel.Risky, t.Risk);
            Assert.True(t.IsReversible, $"{t.Id} no es reversible y está en {preset.Id}");
            Assert.NotEqual(TweakCategory.Debloat, t.Category);
            Assert.NotEqual(TweakCategory.Advanced, t.Category);
        }));
    }

    [Fact]
    public void SafePreset_OnlyHasSafeRecommendedTweaks()
    {
        Assert.All(Get("preset.seguro").Tweaks, t =>
        {
            Assert.Equal(RiskLevel.Safe, t.Risk);
            Assert.True(t.DefaultEnabled);
        });
    }

    [Fact]
    public void MediumPreset_AddsTheRestOfTheSafeOnes_ButNothingAdvanced()
    {
        Assert.All(Get("preset.medio").Tweaks, t => Assert.Equal(RiskLevel.Safe, t.Risk));
    }

    [Fact]
    public void ExtremePreset_AddsAdvancedButKeepsAppPermissionsOut()
    {
        var ids = Get("preset.extremo").Tweaks.Select(t => t.Id).ToList();

        Assert.Contains(Get("preset.extremo").Tweaks, t => t.Risk == RiskLevel.Advanced);
        Assert.All(new[] { "priv.camera", "priv.microphone", "priv.contacts", "priv.calendar",
                           "priv.notifications", "priv.accountinfo" },
            id => Assert.DoesNotContain(id, ids));
    }

    [Fact]
    public void Summary_CountsTweaksByCategory()
    {
        var summary = Get("preset.medio").CountsByCategory();

        Assert.True(summary[TweakCategory.Privacy] > 0);
        Assert.True(summary[TweakCategory.Services] > 0);
        Assert.Equal(Get("preset.medio").Tweaks.Count, summary.Values.Sum());
    }
}
