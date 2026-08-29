using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

public class PowerPlanSettingTweakTests
{
    [Fact]
    public void ParseCurrentSettingIndex_ReturnsTheRequestedPowerMode()
    {
        const string output = "Current AC Power Setting Index: 0x00000000\r\nCurrent DC Power Setting Index: 0x00000002";

        Assert.Equal("0x00000000", PowerPlanSettingTweak.ParseCurrentSettingIndex(output, "AC"));
        Assert.Equal("0x00000002", PowerPlanSettingTweak.ParseCurrentSettingIndex(output, "DC"));
    }
}
