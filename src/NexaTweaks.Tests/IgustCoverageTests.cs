using NexaTweaks.Core.Booster;
using NexaTweaks.Core.Catalog;

namespace NexaTweaks.Tests;

public class IgustCoverageTests
{
    [Fact]
    public void Catalog_IncludesTheMissingReversibleIGustOptions()
    {
        var ids = TweakCatalog.Windows
            .Concat(TweakCatalog.Input)
            .Concat(TweakCatalog.Gpu)
            .Concat(TweakCatalog.Advanced)
            .Select(t => t.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expected = new[]
        {
            "win.transparency",
            "win.backgroundapps",
            "win.errorreporting",
            "win.searchindex",
            "win.startsearch.web",
            "win.powerthrottling",
            "win.pcieaspm",
            "gpu.mpo",
            "gpu.fullscreenoptimizations",
            "gpu.games.priority",
            "gpu.games.scheduling",
            "gpu.games.sfio",
            "adv.programcompat",
            "adv.xbox.gamesave",
            "adv.xbox.network",
            "adv.xbox.gip",
            "win.programtracking",
            "win.telemetry.appdata",
            "win.telemetry.advertising",
            "win.telemetry.consumer",
            "win.biometric",
            "win.printspooler",
            "win.prefetch",
            "input.usbselectivesuspend.power",
            "gpu.gamebar.startup",
            "gpu.gamebar.nexus",
            "gpu.gamebar.capture",
            "gpu.gamemode.auto",
            "gpu.gamemode.allow",
            "gpu.fullscreenoptimizations.honor",
            "gpu.foregroundpriority",
            "adv.nvidiatelemetry",
        };

        Assert.All(expected, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void DefaultProfiles_IncludeTheIGustGamePresetProcesses()
    {
        var root = Path.Combine(Path.GetTempPath(), "NexaTweaksTests_" + Guid.NewGuid().ToString("N"));
        try
        {
            var profiles = new GameProfileStore(root).Load();
            var processes = profiles.Select(p => p.ProcessName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            Assert.All(new[]
            {
                "cs2", "FortniteClient-Win64-Shipping", "GTA5", "FiveM_b2372_GTAProcess",
                "VALORANT-Win64-Shipping", "LeagueClient", "cod", "r5apex", "RobloxPlayerBeta",
                "BF2042", "RDR2", "Cyberpunk2077", "Palworld-Win64-Shipping", "Warframe.x64",
            }, process => Assert.Contains(process, processes));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
