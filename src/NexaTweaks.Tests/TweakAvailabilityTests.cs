using Microsoft.Win32;
using NexaTweaks.Core;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.Tests;

/// <summary>
/// Not every tweak applies to every machine: a service can be absent because Windows removed it
/// (Fax), because the vendor software isn't installed (Google Update), or because the edition
/// doesn't ship it. Offering those as "sin aplicar" makes the user click and get an error.
/// </summary>
public class TweakAvailabilityTests
{
    [Fact]
    public void ServiceTweak_ForAServiceThatDoesNotExist_IsNotAvailable()
    {
        var tweak = new ServiceStateTweak
        {
            Id = "test.missing",
            Name = "Servicio inexistente",
            Description = "",
            Category = TweakCategory.Services,
            ServiceName = "NexaTweaksServicioQueNoExiste",
            DesiredStartMode = "Disabled",
        };

        Assert.False(tweak.IsAvailable());
    }

    [Fact]
    public void ServiceTweak_ForAServiceEveryWindowsHas_IsAvailable()
    {
        var tweak = new ServiceStateTweak
        {
            Id = "test.eventlog",
            Name = "Registro de eventos",
            Description = "",
            Category = TweakCategory.Services,
            ServiceName = "EventLog",
            DesiredStartMode = "Manual",
        };

        Assert.True(tweak.IsAvailable());
    }

    [Fact]
    public void RegistryTweaks_AreAlwaysAvailable()
    {
        var tweak = new RegistryTweak
        {
            Id = "test.registry",
            Name = "Cualquiera",
            Description = "",
            Category = TweakCategory.Windows,
            Hive = RegistryHive.CurrentUser,
            SubKey = @"Software\NexaTweaks.AvailabilityTests",
            ValueName = "x",
            EnabledValue = 1,
        };

        Assert.True(tweak.IsAvailable());
    }

    [Fact]
    public void CatalogTweaks_ReportAvailabilityWithoutThrowing()
    {
        Assert.All(TweakCatalog.All, t =>
        {
            var exception = Record.Exception(() => t.IsAvailable());
            Assert.Null(exception);
        });
    }
}
