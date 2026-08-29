using NexaTweaks.Core.Cleanup;

namespace NexaTweaks.Tests;

public class CarpiCleanupPathsTests
{
    [Fact]
    public void CarpiDerivedCleanupPaths_OnlyTargetRecreatableCaches()
    {
        var paths = CleanupPathCatalog.All();

        Assert.Contains(paths, path => path.EndsWith(@"D3DSCache", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(paths, path => path.EndsWith(@"NVIDIA\DXCache", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(paths, path => path.EndsWith(@"NVIDIA\GLCache", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(paths, path => path.EndsWith(@"AMD\DxCache", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(paths, path => path.EndsWith(@"AMD\GLCache", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(paths, path => path.EndsWith(@"discord\Cache", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(paths, path => path.EndsWith(@"Steam\htmlcache", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(paths, path => path.Contains("WindowsApps", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(paths.Count, paths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
