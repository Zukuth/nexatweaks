using NexaTweaks.Core.Stability;
using Xunit.Abstractions;

namespace NexaTweaks.Tests;

/// <summary>Runs only when NEXA_INTEGRATION=1, because it reads this machine's event log,
/// registry and WMI (results differ per PC and CI runners have little history).</summary>
public sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("NEXA_INTEGRATION") != "1")
            Skip = "Define NEXA_INTEGRATION=1 para leer el sistema real.";
    }
}

public class StabilityIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public StabilityIntegrationTests(ITestOutputHelper output) => _output = output;

    [IntegrationFact]
    public void RealProbe_EveryCheckRunsWithoutErrors()
    {
        var findings = new StabilityAnalyzer().Analyze(new WindowsStabilityProbe(), DateTime.Now);

        foreach (var f in findings)
        {
            _output.WriteLine($"[{f.Severity}] {f.CheckId}: {f.Title}{(f.Fix is null ? "" : $"  (arreglo: {f.Fix.Id})")}");
            foreach (var line in f.Evidence) _output.WriteLine($"    - {line}");
        }

        Assert.DoesNotContain(findings, f => f.Severity == StabilitySeverity.Info);
    }

    [IntegrationFact]
    public void RealProbe_ReadsEventsFilteredByProviderIdAndDate()
    {
        var since = DateTime.Now.AddDays(-30);
        var events = new WindowsStabilityProbe()
            .ReadEvents("System", new[] { "Microsoft-Windows-Kernel-General" }, new[] { 12, 13 }, since);

        Assert.NotEmpty(events);
        Assert.All(events, e =>
        {
            Assert.Equal("Microsoft-Windows-Kernel-General", e.Provider);
            Assert.Contains(e.Id, new[] { 12, 13 });
            Assert.True(e.Time >= since);
        });
        Assert.True(events.Zip(events.Skip(1)).All(p => p.First.Time >= p.Second.Time), "Deben venir del más nuevo al más viejo.");
    }
}
