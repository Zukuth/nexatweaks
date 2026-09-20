using System.Diagnostics;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

public class ServiceStartModesTests
{
    [Fact]
    public void Snapshot_ListsTheServicesOfThisMachine()
    {
        var snapshot = ServiceStartModes.Snapshot();

        Assert.NotEmpty(snapshot);
        Assert.True(snapshot.ContainsKey("EventLog"), "Todo Windows tiene el servicio EventLog");
    }

    [Fact]
    public void Get_IsCaseInsensitive_AndNullForAMissingService()
    {
        Assert.NotNull(ServiceStartModes.Get("eventlog"));
        Assert.Null(ServiceStartModes.Get("NexaTweaksServicioQueNoExiste"));
    }

    [Fact]
    public void Snapshot_IsCached_SoOpeningThePageDoesNotHitWmiPerService()
    {
        ServiceStartModes.Invalidate();
        ServiceStartModes.Snapshot();   // primera carga: paga la consulta WMI

        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 200; i++) ServiceStartModes.Get("EventLog");
        watch.Stop();

        Assert.True(watch.ElapsedMilliseconds < 200,
            $"200 lecturas cacheadas tardaron {watch.ElapsedMilliseconds} ms: parece que consultó WMI en cada una");
    }
}
