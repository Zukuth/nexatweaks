using NexaTweaks.Core.Catalog;

namespace NexaTweaks.Tests;

public class SecurityCatalogTests
{
    [Fact]
    public void AdvancedCatalog_DoesNotOfferToDisableWindowsSecurityControls()
    {
        var ids = TweakCatalog.Advanced
            .Select(tweak => tweak.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("adv.defender.realtime", ids);
        Assert.DoesNotContain("adv.coreisolation", ids);
        Assert.DoesNotContain("adv.uac.level", ids);
        Assert.DoesNotContain("adv.windowsupdate", ids);
    }
}
