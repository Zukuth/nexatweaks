using NexaTweaks.Core;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

/// <summary>Guards the catalog added after comparing NexaTweaks with other optimizers: the ids
/// must exist, stay unique, and never offer to break Windows Update or Defender.</summary>
public class CatalogExpansionTests
{
    private static IReadOnlyList<ITweak> All => TweakCatalog.All;

    private static IEnumerable<string> Ids(IEnumerable<ITweak> tweaks) => tweaks.Select(t => t.Id);

    [Fact]
    public void All_ContainsEveryCategory_WithUniqueIds()
    {
        var ids = Ids(All).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
        Assert.All(new[] { TweakCategory.Windows, TweakCategory.Network, TweakCategory.Input, TweakCategory.Gpu,
                           TweakCategory.Cleanup, TweakCategory.Advanced, TweakCategory.Services,
                           TweakCategory.Privacy, TweakCategory.Interface },
            category => Assert.Contains(All, t => t.Category == category));
    }

    [Fact]
    public void Every_TweakHasNameAndDescription()
    {
        Assert.All(All, t =>
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Name), $"{t.Id} sin nombre");
            Assert.False(string.IsNullOrWhiteSpace(t.Description), $"{t.Id} sin descripción");
        });
    }

    [Fact]
    public void RiskyTweaks_AreNeverEnabledByDefault()
    {
        Assert.All(All.Where(t => t.Risk != RiskLevel.Safe), t =>
            Assert.False(t.DefaultEnabled, $"{t.Id} es {t.Risk} y viene activado por defecto"));
    }

    // --- Servicios ----------------------------------------------------------------------

    [Fact]
    public void Services_IncludeTheOnesSafeToTurnOffOnAHomePc()
    {
        var ids = Ids(TweakCatalog.Services).ToList();

        Assert.All(new[]
        {
            "svc.remoteregistry", "svc.fax", "svc.retaildemo", "svc.mapsbroker", "svc.wmpnetworksvc",
            "svc.p2psvc", "svc.p2pimsvc", "svc.pnrpsvc", "svc.pnrpautoreg", "svc.ajrouter",
            "svc.wersvc", "svc.pcasvc", "svc.cscservice", "svc.snmptrap", "svc.lltdsvc",
            "svc.rpclocator", "svc.trkwks", "svc.ndu", "svc.nvtelemetry", "svc.gupdate",
            "svc.gupdatem", "svc.wisvc", "svc.wpcmonsvc", "svc.remoteaccess", "svc.printnotify",
            "svc.stisvc", "svc.dosvc",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Services_NeverTouchWindowsUpdateOrSecurity()
    {
        var services = TweakCatalog.Services.OfType<ServiceStateTweak>().Select(t => t.ServiceName).ToList();

        Assert.All(new[] { "wuauserv", "WaaSMedicSvc", "BITS", "CryptSvc", "wscsvc", "WinDefend", "mpssvc", "EventLog" },
            name => Assert.DoesNotContain(name, services, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void Services_AreServiceTweaksWithAValidStartMode()
    {
        Assert.All(TweakCatalog.Services, t =>
        {
            var service = Assert.IsType<ServiceStateTweak>(t);
            Assert.Contains(service.DesiredStartMode, new[] { "Disabled", "Manual" });
            Assert.False(string.IsNullOrWhiteSpace(service.ServiceName));
        });
    }

    [Fact]
    public void DeliveryOptimizationAndScanner_AreSetToManual_NotDisabled()
    {
        var byId = TweakCatalog.Services.OfType<ServiceStateTweak>().ToDictionary(t => t.Id);

        Assert.Equal("Manual", byId["svc.dosvc"].DesiredStartMode);
        Assert.Equal("Manual", byId["svc.stisvc"].DesiredStartMode);
    }

    // --- Privacidad ---------------------------------------------------------------------

    [Fact]
    public void Privacy_CoversTheModernWindowsDataCollection()
    {
        var ids = Ids(TweakCatalog.Privacy).ToList();

        Assert.All(new[]
        {
            "priv.copilot", "priv.recall", "priv.widgets", "priv.wifisense", "priv.inkcollection",
            "priv.inputpersonalization", "priv.siuf", "priv.feedbackfrequency", "priv.tailoredexperiences",
            "priv.consumerfeatures", "priv.cloudcontent", "priv.suggestedapps", "priv.subscribedcontent",
            "priv.oempreinstalled", "priv.appdiagnostics", "priv.camera", "priv.microphone",
            "priv.contacts", "priv.calendar", "priv.notifications", "priv.accountinfo",
            "priv.clipboardhistory", "priv.speechcloud",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void Privacy_AppPermissionBlocks_AreAdvancedAndOffByDefault()
    {
        var permissions = TweakCatalog.Privacy
            .Where(t => t.Id is "priv.camera" or "priv.microphone" or "priv.contacts" or "priv.calendar"
                        or "priv.notifications" or "priv.accountinfo")
            .ToList();

        Assert.Equal(6, permissions.Count);
        Assert.All(permissions, t =>
        {
            Assert.Equal(RiskLevel.Advanced, t.Risk);
            Assert.False(t.DefaultEnabled);
            Assert.Equal(2, Assert.IsType<RegistryTweak>(t).EnabledValue);
        });
    }

    // --- Interfaz -----------------------------------------------------------------------

    [Fact]
    public void Interface_CoversTaskbarAndExplorerBasics()
    {
        var ids = Ids(TweakCatalog.Interface).ToList();

        Assert.All(new[]
        {
            "ui.taskbar.left", "ui.taskbar.search", "ui.taskbar.taskview", "ui.taskbar.chat",
            "ui.explorer.extensions", "ui.explorer.hidden", "ui.explorer.thispc",
            "ui.verbosestatus", "ui.menushowdelay",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void ShowFileExtensions_TurnsTheHidingOff()
    {
        var tweak = Assert.IsType<RegistryTweak>(TweakCatalog.Interface.Single(t => t.Id == "ui.explorer.extensions"));

        Assert.Equal("HideFileExt", tweak.ValueName);
        Assert.Equal(0, tweak.EnabledValue);
    }

    // --- Rendimiento y red --------------------------------------------------------------

    [Fact]
    public void Windows_GainsTheResponsivenessTweaks()
    {
        var ids = Ids(TweakCatalog.Windows).ToList();

        Assert.All(new[]
        {
            "win.waittokillapp", "win.hungapptimeout", "win.autoendtasks", "win.waittokillservice",
            "win.pagingexecutive", "win.foregroundlock", "win.timerresolution", "win.prioritycontrol",
        }, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void TimeoutTweaks_AreWrittenAsStrings_BecauseWindowsReadsThemThatWay()
    {
        var byId = TweakCatalog.Windows.OfType<RegistryTweak>().ToDictionary(t => t.Id);

        Assert.All(new[] { "win.waittokillapp", "win.hungapptimeout", "win.autoendtasks", "win.waittokillservice" },
            id => Assert.Equal(Microsoft.Win32.RegistryValueKind.String, byId[id].ValueKind));
    }

    [Fact]
    public void Network_GainsTcpAndDnsTuning()
    {
        var ids = Ids(TweakCatalog.Network).ToList();

        Assert.All(new[] { "net.tcp.pmtu", "net.tcp.retransmissions", "net.dnspriority", "net.localpriority" },
            id => Assert.Contains(id, ids));
    }
}
