using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.Tests;

public class SystemOverviewTests
{
    [Fact]
    public void WindowsBadges_ShowSecurityFeaturesAsGoodOrWarning()
    {
        var badges = SystemOverviewFactory.WindowsBadges(vbsEnabled: false, hvciEnabled: true,
            displayVersion: "25H2", build: "26200.9445");

        Assert.Equal(new[] { "VBS desactivado", "HVCI activado", "25H2", "Build 26200.9445" },
            badges.Select(b => b.Text));
        Assert.Equal(BadgeTone.Warn, badges[0].Tone);
        Assert.Equal(BadgeTone.Good, badges[1].Tone);
        Assert.Equal(BadgeTone.Neutral, badges[3].Tone);
    }

    [Fact]
    public void WindowsBadges_OmitVersionWhenUnknown()
    {
        var badges = SystemOverviewFactory.WindowsBadges(true, true, displayVersion: "", build: "");

        Assert.Equal(new[] { "VBS activado", "HVCI activado" }, badges.Select(b => b.Text));
    }

    [Fact]
    public void BoardBadges_ReportTpmSecureBootAndBiosYear()
    {
        var badges = SystemOverviewFactory.BoardBadges(tpmVersion: "2.0", secureBoot: true,
            biosVersion: "B550M.2801", biosDate: new DateTime(2022, 5, 31));

        Assert.Equal(new[] { "TPM 2.0", "Secure Boot activado", "BIOS B550M.2801", "2022" },
            badges.Select(b => b.Text));
        Assert.Equal(BadgeTone.Good, badges[0].Tone);
        Assert.Equal(BadgeTone.Good, badges[1].Tone);
    }

    [Fact]
    public void BoardBadges_WarnWhenSecureBootIsOffOrTpmMissing()
    {
        var badges = SystemOverviewFactory.BoardBadges(tpmVersion: null, secureBoot: false,
            biosVersion: "", biosDate: null);

        Assert.Equal(new[] { "Sin TPM", "Secure Boot desactivado" }, badges.Select(b => b.Text));
        Assert.All(badges, b => Assert.Equal(BadgeTone.Warn, b.Tone));
    }

    [Fact]
    public void BoardBadges_MarkBiosOlderThanThreeYearsAsWarning()
    {
        var old = SystemOverviewFactory.BoardBadges(null, null, "X.1", DateTime.Now.AddYears(-4));
        var recent = SystemOverviewFactory.BoardBadges(null, null, "X.9", DateTime.Now.AddMonths(-6));

        Assert.Equal(BadgeTone.Warn, old.Single(b => b.Text == DateTime.Now.AddYears(-4).Year.ToString()).Tone);
        Assert.Equal(BadgeTone.Neutral, recent.Single(b => b.Text == DateTime.Now.AddMonths(-6).Year.ToString()).Tone);
    }

    [Fact]
    public void BoardBadges_SecureBootUnknownIsNotReported()
    {
        var badges = SystemOverviewFactory.BoardBadges(tpmVersion: "2.0", secureBoot: null, biosVersion: "", biosDate: null);

        Assert.DoesNotContain(badges, b => b.Text.Contains("Secure Boot"));
    }

    [Theory]
    [InlineData(17179869184, "16 GB")]
    [InlineData(8589934592, "8 GB")]
    public void RamModule_DescribesCapacityInGb(ulong bytes, string expected)
    {
        var module = SystemOverviewFactory.RamModule("P0 CHANNEL A", "Kingston", bytes, 2667);

        Assert.Equal(expected, module.CapacityText);
        Assert.Equal("2667 MT/s", module.SpeedText);
        Assert.Equal("P0 CHANNEL A", module.Slot);
        Assert.Equal("Kingston", module.Vendor);
    }

    [Fact]
    public void RamModule_FallsBackWhenVendorIsMissing()
    {
        var module = SystemOverviewFactory.RamModule("", "   ", 8589934592, 0);

        Assert.Equal("DIMM", module.Slot);
        Assert.Equal("Desconocido", module.Vendor);
        Assert.Equal("", module.SpeedText);
    }
}
