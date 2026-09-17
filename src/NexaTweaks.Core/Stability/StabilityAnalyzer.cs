namespace NexaTweaks.Core.Stability;

/// <summary>Runs every stability check against one probe. A check that fails (missing
/// permissions, a disabled WMI class, ...) becomes an Info finding instead of aborting the scan.</summary>
public sealed class StabilityAnalyzer
{
    public static IReadOnlyList<IStabilityCheck> DefaultChecks { get; } = new IStabilityCheck[]
    {
        new BugCheckCheck(),
        new UnexpectedShutdownCheck(),
        new TpmErrorCheck(),
        new HardwareErrorCheck(),
        new AppCrashCheck(),
        new MemoryTestCheck(),
        new MixedRamCheck(),
        new TdrDelayCheck(),
        new WindowsUpdatePolicyCheck(),
        new LaptopThrottlingCheck(),
        new OldDriversCheck(),
        new OldBiosCheck(),
        new ModdedWindowsCheck(),
        new RiskyKernelDriversCheck(),
    };

    private readonly IReadOnlyList<IStabilityCheck> _checks;

    public StabilityAnalyzer(IEnumerable<IStabilityCheck>? checks = null) =>
        _checks = (checks ?? DefaultChecks).ToList();

    public IReadOnlyList<StabilityFinding> Analyze(IStabilityProbe probe, DateTime now)
    {
        var findings = new List<StabilityFinding>();
        foreach (var check in _checks)
        {
            try
            {
                findings.AddRange(check.Run(probe, now));
            }
            catch (Exception ex)
            {
                findings.Add(new StabilityFinding(check.Id, $"No se pudo verificar: {check.Name}",
                    StabilitySeverity.Info, new[] { ex.Message },
                    "Ejecuta NexaTweaks como administrador y vuelve a analizar."));
            }
        }

        return findings.OrderByDescending(f => f.Severity).ToList();
    }
}
