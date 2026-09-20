using NexaTweaks.Core.Stability;
using NexaTweaks.Tests.Fakes;

namespace NexaTweaks.Tests;

public class StabilityAnalyzerTests
{
    private static readonly DateTime Now = new(2026, 9, 17);

    private sealed class StubCheck : IStabilityCheck
    {
        private readonly Func<IEnumerable<StabilityFinding>> _run;
        public StubCheck(string id, Func<IEnumerable<StabilityFinding>> run) { Id = id; _run = run; }
        public string Id { get; }
        public string Name => $"Stub {Id}";
        public IEnumerable<StabilityFinding> Run(IStabilityProbe probe, DateTime now) => _run();
    }

    private static StabilityFinding Finding(string id, StabilitySeverity severity) =>
        new(id, id, severity, Array.Empty<string>(), "");

    [Fact]
    public void Analyze_OrdersFindingsFromMostToLeastSevere()
    {
        var analyzer = new StabilityAnalyzer(new[]
        {
            new StubCheck("low", () => new[] { Finding("low", StabilitySeverity.Low) }),
            new StubCheck("high", () => new[] { Finding("high", StabilitySeverity.High) }),
            new StubCheck("medium", () => new[] { Finding("medium", StabilitySeverity.Medium) }),
        });

        var findings = analyzer.Analyze(new FakeStabilityProbe(), Now);

        Assert.Equal(new[] { "high", "medium", "low" }, findings.Select(f => f.CheckId));
    }

    [Fact]
    public void Analyze_WhenACheckThrows_ReportsItAndKeepsGoing()
    {
        var analyzer = new StabilityAnalyzer(new[]
        {
            new StubCheck("broken", () => throw new UnauthorizedAccessException("Acceso denegado")),
            new StubCheck("ok", () => new[] { Finding("ok", StabilitySeverity.High) }),
        });

        var findings = analyzer.Analyze(new FakeStabilityProbe(), Now);

        Assert.Equal(2, findings.Count);
        var failed = Assert.Single(findings, f => f.CheckId == "broken");
        Assert.Equal(StabilitySeverity.Info, failed.Severity);
        Assert.Contains(failed.Evidence, e => e.Contains("Acceso denegado"));
    }

    [Fact]
    public void DefaultChecks_HaveUniqueIds()
    {
        var ids = StabilityAnalyzer.DefaultChecks.Select(c => c.Id).ToList();

        Assert.Equal(14, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Fixes_AreReversibleAndHaveUniqueStabilityIds()
    {
        var ids = StabilityFixes.All.Select(t => t.Id).ToList();

        Assert.NotEmpty(ids);
        Assert.All(StabilityFixes.All, t => Assert.True(t.IsReversible));
        Assert.All(ids, id => Assert.StartsWith("stability.", id));
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }
}
