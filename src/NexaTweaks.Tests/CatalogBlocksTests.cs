using NexaTweaks.Core;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Repair;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

/// <summary>Second wave of catalog work: interfaz, IA, permisos restantes, navegadores,
/// mantenimiento y más debloat.</summary>
public class CatalogBlocksTests
{
    private static IEnumerable<string> Ids(IEnumerable<ITweak> tweaks) => tweaks.Select(t => t.Id);

    [Fact]
    public void Interface_GainsTheWindows11Comforts()
    {
        var ids = Ids(TweakCatalog.Interface).ToList();

        Assert.All(new[]
        {
            "ui.classicmenu", "ui.darkmode.apps", "ui.darkmode.system", "ui.nolockscreen",
            "ui.loginblur", "ui.spotlight", "ui.startmenurecs", "ui.endtask", "ui.detailedbsod",
            "ui.iconsonly", "ui.numlock", "ui.stickykeys", "ui.snapflyout",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void ClassicContextMenu_EmptiesTheBlockingClsid()
    {
        var tweak = Assert.IsType<RegistryTweak>(TweakCatalog.Interface.Single(t => t.Id == "ui.classicmenu"));

        Assert.Contains("{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}", tweak.SubKey);
        Assert.Equal("", tweak.ValueName);
        Assert.Equal("", tweak.EnabledValue);
    }

    [Fact]
    public void Privacy_TurnsOffTheWindowsAiFeatures_WithMicrosoftsOwnPolicies()
    {
        var byId = TweakCatalog.Privacy.OfType<RegistryTweak>().ToDictionary(t => t.Id);

        Assert.Equal(@"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", byId["priv.ai.clicktodo"].SubKey);
        Assert.Equal("DisableClickToDo", byId["priv.ai.clicktodo"].ValueName);
        Assert.Equal(@"SOFTWARE\Policies\WindowsNotepad", byId["priv.ai.notepad"].SubKey);
        Assert.Equal("DisableAIFeatures", byId["priv.ai.notepad"].ValueName);
        Assert.Equal(@"Software\Microsoft\Windows\CurrentVersion\Policies\Paint", byId["priv.ai.paint.cocreator"].SubKey);
        Assert.Equal("DisableGenerativeFill", byId["priv.ai.paint.fill"].ValueName);
    }

    [Fact]
    public void Privacy_CoversTheRemainingAppPermissions()
    {
        var ids = Ids(TweakCatalog.Privacy).ToList();

        Assert.All(new[]
        {
            "priv.messaging", "priv.email", "priv.callhistory", "priv.phone", "priv.tasks",
            "priv.motion", "priv.radios", "priv.location.apps", "priv.trusteddevices",
            "priv.devicediscovery", "priv.filesystem", "priv.voiceactivation",
            "priv.capture.border", "priv.capture.programmatic",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Privacy_AddsThirdPartyAndCloudTelemetry()
    {
        var ids = Ids(TweakCatalog.Privacy).ToList();

        Assert.All(new[] { "priv.office.telemetry", "priv.mediaplayer.metadata", "priv.search.cloud",
                           "priv.findmydevice", "priv.cloudsync" },
            id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Windows_AddsFileSystemAndPowerTuning()
    {
        var ids = Ids(TweakCatalog.Windows).ToList();

        Assert.All(new[] { "win.ntfs.lastaccess", "win.longpaths", "win.modernstandby",
                           "win.utctime", "win.reservedstorage" },
            id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Network_AddsTeredo()
    {
        Assert.Contains("net.teredo", Ids(TweakCatalog.Network));
    }

    // --- Navegadores --------------------------------------------------------------------

    [Fact]
    public void Browsers_CoverEdgeChromeAndBrave()
    {
        var ids = Ids(TweakCatalog.Browsers).ToList();

        Assert.All(new[]
        {
            "browser.edge.telemetry", "browser.edge.sync", "browser.edge.personalization",
            "browser.edge.feedback", "browser.edge.shopping", "browser.edge.startupboost",
            "browser.edge.background", "browser.chrome.telemetry", "browser.chrome.sync",
            "browser.chrome.background", "browser.chrome.urlcollection", "browser.brave.telemetry",
            "browser.brave.sync", "browser.brave.rewards", "browser.brave.wallet", "browser.brave.ai",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Browsers_OnlyTouchOfficialPolicyKeys()
    {
        Assert.All(TweakCatalog.Browsers.OfType<RegistryTweak>(), t =>
        {
            Assert.Contains(@"SOFTWARE\Policies\", t.SubKey, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(TweakCategory.Browsers, t.Category);
        });
    }

    [Fact]
    public void Browsers_NeverDisableSecurityFeatures()
    {
        var values = TweakCatalog.Browsers.OfType<RegistryTweak>().Select(t => t.ValueName).ToList();

        Assert.All(new[] { "SafeBrowsingEnabled", "SafeBrowsingProtectionLevel", "SmartScreenEnabled",
                           "PasswordManagerEnabled", "SSLErrorOverrideAllowed" },
            forbidden => Assert.DoesNotContain(forbidden, values));
    }

    // --- Debloat y mantenimiento --------------------------------------------------------

    [Fact]
    public void Debloat_GrowsWithTheRestOfThePreinstalledApps()
    {
        var ids = Ids(TweakCatalog.Debloat).ToList();

        Assert.All(new[]
        {
            "app.cortana", "app.bingfinance", "app.bingsports", "app.bingtranslator",
            "app.officehub", "app.teams", "app.devhome", "app.outlooknew", "app.copilot",
            "app.journal", "app.whiteboard", "app.powerautomate", "app.print3d",
            "app.speedtest", "app.remotedesktop", "app.readinglist", "app.messaging",
            "app.oneconnect", "app.wallet", "app.powerbi", "app.onedrive",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void OneDrive_UsesItsOwnUninstaller_AndSaysItIsNotReversible()
    {
        var tweak = TweakCatalog.Debloat.Single(t => t.Id == "app.onedrive");

        Assert.False(tweak.IsReversible);
        Assert.Equal(TweakCategory.Debloat, tweak.Category);
        Assert.Equal(RiskLevel.Advanced, tweak.Risk);
    }

    [Fact]
    public void Repair_AddsTheEverydayMaintenanceActions()
    {
        var ids = RepairCatalog.Actions.Select(a => a.Id).ToList();

        Assert.All(new[] { "repair.cleanmgr", "repair.temp", "repair.recyclebin",
                           "repair.network.full", "repair.spooler" },
            id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Repair_NeverTouchesDefenderExclusionsOrWindowsUpdatePolicies()
    {
        var commands = RepairCatalog.Actions.Select(a => $"{a.Executable} {a.Arguments}").ToList();

        Assert.All(new[] { "MpPreference", "ExclusionPath", "TrustedInstaller", "sc delete wuauserv" },
            forbidden => Assert.DoesNotContain(commands, c => c.Contains(forbidden, StringComparison.OrdinalIgnoreCase)));
    }
}
