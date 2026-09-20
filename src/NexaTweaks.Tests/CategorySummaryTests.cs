using NexaTweaks.Core.Catalog;

namespace NexaTweaks.Tests;

public class CategorySummaryTests
{
    [Fact]
    public void WhileChecking_OnlyTheTotalIsKnown()
    {
        Assert.Equal("27 ajustes · comprobando estado...",
            CategorySummary.Format(total: 27, applied: 0, unavailable: 0, isChecking: true));
    }

    [Fact]
    public void AfterChecking_ShowsAppliedOutOfTotal()
    {
        Assert.Equal("27 ajustes · 5 aplicados",
            CategorySummary.Format(total: 27, applied: 5, unavailable: 0, isChecking: false));
    }

    [Fact]
    public void UnavailableOnesAreCalledOut()
    {
        Assert.Equal("27 ajustes · 5 aplicados · 11 no disponibles en este equipo",
            CategorySummary.Format(total: 27, applied: 5, unavailable: 11, isChecking: false));
    }

    [Fact]
    public void SingularReadsNaturally()
    {
        Assert.Equal("1 ajuste · 1 aplicado · 1 no disponible en este equipo",
            CategorySummary.Format(total: 1, applied: 1, unavailable: 1, isChecking: false));
    }

    [Fact]
    public void NothingApplied_StillShowsTheZero()
    {
        Assert.Equal("9 ajustes · 0 aplicados",
            CategorySummary.Format(total: 9, applied: 0, unavailable: 0, isChecking: false));
    }
}
