using NexaTweaks.Core.Catalog;

namespace NexaTweaks.Tests;

public class CarpiCatalogCoverageTests
{
    [Fact]
    public void CleanupCatalog_IncludesSafeCarpiDerivedCacheActions()
    {
        var ids = TweakCatalog.Cleanup.Select(tweak => tweak.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.All(new[]
        {
            "clean.shadercache",
            "clean.discordcache",
            "clean.launchercache",
            "clean.crashreports",
            "clean.developercache",
        }, id => Assert.Contains(id, ids));
    }
}
