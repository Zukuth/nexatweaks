using Microsoft.Win32;
using NexaTweaks.Core.Stability;
using NexaTweaks.Core.Tweaks;
using NexaTweaks.Tests.Fakes;

namespace NexaTweaks.Tests;

public class StabilityChecksTests
{
    private static readonly DateTime Now = new(2026, 9, 17, 12, 0, 0);

    private const string GraphicsDrivers = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";
    private const string WuPolicies = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
    private const string PowerThrottling = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling";
    private const string CurrentVersion = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private static List<StabilityFinding> Run(IStabilityCheck check, FakeStabilityProbe probe) =>
        check.Run(probe, Now).ToList();

    // --- Event log checks ---------------------------------------------------------------

    [Fact]
    public void BugCheck_GroupsCrashesByStopCode_AndNamesThem()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-WER-SystemErrorReporting", 1001,
                "0x0000003b (0x00000000c0000005, 0xfffff80275b75a1e, 0xffff838424f82ad0, 0x0000000000000000)")
            .AddEvent("System", Now.AddDays(-2), "Microsoft-Windows-WER-SystemErrorReporting", 1001,
                "0x00000139 (0x0000000000000021, 0xfffff80710d28b00, 0xfffff80710d28a58, 0x0000000000000000)")
            .AddEvent("System", Now.AddDays(-3), "Microsoft-Windows-WER-SystemErrorReporting", 1001,
                "0x0000003b (0x00000000c0000005, 0x0, 0x0, 0x0)");

        var finding = Assert.Single(Run(new BugCheckCheck(), probe));

        Assert.Equal("bugcheck", finding.CheckId);
        Assert.Equal(StabilitySeverity.High, finding.Severity);
        Assert.Contains("3", finding.Title);
        Assert.Contains(finding.Evidence, e => e.Contains("0x3B") && e.Contains("SYSTEM_SERVICE_EXCEPTION") && e.Contains("2"));
        Assert.Contains(finding.Evidence, e => e.Contains("0x139") && e.Contains("KERNEL_SECURITY_CHECK_FAILURE"));
        Assert.Null(finding.Fix);
    }

    [Fact]
    public void BugCheck_WithThreeOrMoreDistinctCodes_PointsToSystemWideCause()
    {
        var probe = new FakeStabilityProbe();
        foreach (var code in new[] { "0x0000003b", "0x00000139", "0x0000009f" })
            probe.AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-WER-SystemErrorReporting", 1001, $"{code} (0x0)");

        var finding = Assert.Single(Run(new BugCheckCheck(), probe));

        Assert.Contains("RAM", finding.Recommendation);
    }

    [Fact]
    public void BugCheck_IgnoresCrashesOlderThan30Days()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-45), "Microsoft-Windows-WER-SystemErrorReporting", 1001, "0x0000000a (0x0)");

        Assert.Empty(Run(new BugCheckCheck(), probe));
    }

    [Fact]
    public void UnexpectedShutdown_ReportsOnlyShutdownsWithoutStopCode()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-Kernel-Power", 41, 59L)
            .AddEvent("System", Now.AddDays(-4), "Microsoft-Windows-Kernel-Power", 41, 0L)
            .AddEvent("System", Now.AddDays(-6), "Microsoft-Windows-Kernel-Power", 41, 0L);

        var finding = Assert.Single(Run(new UnexpectedShutdownCheck(), probe));

        Assert.Equal(StabilitySeverity.Medium, finding.Severity);
        Assert.Contains("2", finding.Title);
    }

    [Fact]
    public void UnexpectedShutdown_WhenAllHadStopCode_ReportsNothing()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-Kernel-Power", 41, 59L);

        Assert.Empty(Run(new UnexpectedShutdownCheck(), probe));
    }

    [Fact]
    public void Tpm_FatalErrorEvent_IsHighSeverity()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-3), "TPM", 14)
            .AddEvent("System", Now.AddDays(-2), "TPM", 14);

        var finding = Assert.Single(Run(new TpmErrorCheck(), probe));

        Assert.Equal(StabilitySeverity.High, finding.Severity);
        Assert.Contains("BIOS", finding.Recommendation);
    }

    [Fact]
    public void Whea_HardwareErrorEvent_IsHighSeverity()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-WHEA-Logger", 17);

        var finding = Assert.Single(Run(new HardwareErrorCheck(), probe));

        Assert.Equal("whea", finding.CheckId);
        Assert.Equal(StabilitySeverity.High, finding.Severity);
    }

    [Fact]
    public void AppCrash_GroupsByAppAndFaultingModule_FromThreeCrashes()
    {
        var probe = new FakeStabilityProbe();
        for (var i = 0; i < 13; i++)
            probe.AddEvent("Application", Now.AddHours(-i), "Application Error", 1000,
                "AsusSplendid.exe", "2.1.54.0", "6a7c1e50", "amdadlx64.dll");
        probe.AddEvent("Application", Now.AddHours(-1), "Application Error", 1000,
            "notepad.exe", "1.0", "0", "ntdll.dll");

        var finding = Assert.Single(Run(new AppCrashCheck(), probe));

        Assert.Equal(StabilitySeverity.Medium, finding.Severity);
        Assert.Contains("AsusSplendid.exe", finding.Title);
        Assert.Contains("13", finding.Title);
        Assert.Contains(finding.Evidence, e => e.Contains("amdadlx64.dll"));
        Assert.Contains("AMD", finding.Recommendation);
    }

    [Fact]
    public void MemoryTest_WithErrors_IsHighSeverity()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-10), "Microsoft-Windows-MemoryDiagnostics-Results", 1102);

        var finding = Assert.Single(Run(new MemoryTestCheck(), probe));

        Assert.Equal(StabilitySeverity.High, finding.Severity);
    }

    [Fact]
    public void MemoryTest_NeverRunButRecentBlueScreens_SuggestsRunningIt()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-WER-SystemErrorReporting", 1001, "0x0000003b (0x0)");

        var finding = Assert.Single(Run(new MemoryTestCheck(), probe));

        Assert.Equal(StabilitySeverity.Medium, finding.Severity);
        Assert.Contains("mdsched", finding.Recommendation);
    }

    [Fact]
    public void MemoryTest_LatestRunPassed_ReportsNothing()
    {
        var probe = new FakeStabilityProbe()
            .AddEvent("System", Now.AddDays(-20), "Microsoft-Windows-MemoryDiagnostics-Results", 1102)
            .AddEvent("System", Now.AddDays(-5), "Microsoft-Windows-MemoryDiagnostics-Results", 1101)
            .AddEvent("System", Now.AddDays(-1), "Microsoft-Windows-WER-SystemErrorReporting", 1001, "0x0000003b (0x0)");

        Assert.Empty(Run(new MemoryTestCheck(), probe));
    }

    // --- Configuration checks (with fixes) ----------------------------------------------

    [Fact]
    public void Tdr_AbnormalDelay_OffersFixBackToDefault()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, GraphicsDrivers, "TdrDelay", 360);

        var finding = Assert.Single(Run(new TdrDelayCheck(), probe));

        Assert.Equal(StabilitySeverity.High, finding.Severity);
        Assert.Contains(finding.Evidence, e => e.Contains("360"));
        var fix = Assert.IsType<RegistryTweak>(finding.Fix);
        Assert.Equal("TdrDelay", fix.ValueName);
        Assert.Equal(2, fix.EnabledValue);
        Assert.Equal(GraphicsDrivers, fix.SubKey);
    }

    [Fact]
    public void Tdr_AbnormalDdiDelay_OffersFixBackToDefault()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, GraphicsDrivers, "TdrDdiDelay", 60);

        var finding = Assert.Single(Run(new TdrDelayCheck(), probe));

        var fix = Assert.IsType<RegistryTweak>(finding.Fix);
        Assert.Equal("TdrDdiDelay", fix.ValueName);
        Assert.Equal(5, fix.EnabledValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    [InlineData(10)]
    public void Tdr_DefaultOrReasonableDelay_ReportsNothing(int? value)
    {
        var probe = new FakeStabilityProbe();
        if (value is not null) probe.SetRegistry(RegistryHive.LocalMachine, GraphicsDrivers, "TdrDelay", value.Value);

        Assert.Empty(Run(new TdrDelayCheck(), probe));
    }

    [Fact]
    public void WindowsUpdatePolicies_EachBlockingPolicy_GetsItsOwnFix()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, WuPolicies, "DisableWUfBSafeguards", 1)
            .SetRegistry(RegistryHive.LocalMachine, WuPolicies, "ExcludeWUDriversInQualityUpdate", 1);

        var findings = Run(new WindowsUpdatePolicyCheck(), probe);

        Assert.Equal(2, findings.Count);
        Assert.All(findings, f => Assert.Equal(0, Assert.IsType<RegistryTweak>(f.Fix).EnabledValue));
        Assert.Contains(findings, f => ((RegistryTweak)f.Fix!).ValueName == "DisableWUfBSafeguards");
        Assert.Contains(findings, f => ((RegistryTweak)f.Fix!).ValueName == "ExcludeWUDriversInQualityUpdate");
    }

    [Fact]
    public void WindowsUpdatePolicies_WhenAbsent_ReportsNothing()
    {
        Assert.Empty(Run(new WindowsUpdatePolicyCheck(), new FakeStabilityProbe()));
    }

    [Fact]
    public void LaptopThrottling_DisabledOnLaptop_OffersFix()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, PowerThrottling, "PowerThrottlingOff", 1)
            .AddWmi("Win32_Battery", new() { ["Name"] = "A32-K55" });

        var finding = Assert.Single(Run(new LaptopThrottlingCheck(), probe));

        Assert.Equal(StabilitySeverity.Low, finding.Severity);
        var fix = Assert.IsType<RegistryTweak>(finding.Fix);
        Assert.Equal("PowerThrottlingOff", fix.ValueName);
        Assert.Equal(0, fix.EnabledValue);
    }

    [Fact]
    public void LaptopThrottling_OnDesktop_ReportsNothing()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, PowerThrottling, "PowerThrottlingOff", 1);

        Assert.Empty(Run(new LaptopThrottlingCheck(), probe));
    }

    // --- Hardware / inventory checks ----------------------------------------------------

    [Fact]
    public void MixedRam_DifferentVendorsAndSpeeds_IsReported()
    {
        var probe = new FakeStabilityProbe()
            .AddWmi("Win32_PhysicalMemory", Ram("P0 CHANNEL A", "Kingston", "9905700-120.A00G", 16, 2667))
            .AddWmi("Win32_PhysicalMemory", Ram("P0 CHANNEL B", "Samsung", "M471A1K43DB1-CWE", 8, 3200));

        var finding = Assert.Single(Run(new MixedRamCheck(), probe));

        Assert.Equal(StabilitySeverity.Medium, finding.Severity);
        Assert.Equal(2, finding.Evidence.Count);
        Assert.Contains(finding.Evidence, e => e.Contains("Kingston") && e.Contains("2667"));
    }

    [Fact]
    public void MixedRam_IdenticalKit_ReportsNothing()
    {
        var probe = new FakeStabilityProbe()
            .AddWmi("Win32_PhysicalMemory", Ram("A", "Samsung", "M471A1K43DB1-CWE ", 8, 3200))
            .AddWmi("Win32_PhysicalMemory", Ram("B", "Samsung", "M471A1K43DB1-CWE", 8, 3200));

        Assert.Empty(Run(new MixedRamCheck(), probe));
    }

    [Fact]
    public void OldDrivers_ListsOnlyVendorDisplayAndNetworkDriversOlderThan3Years()
    {
        var probe = new FakeStabilityProbe()
            .AddWmi("Win32_PnPSignedDriver", Driver("DISPLAY", "AMD Radeon(TM) Graphics", "Advanced Micro Devices, Inc.", "31.0.12014.6", "20221206000000.******+***"))
            .AddWmi("Win32_PnPSignedDriver", Driver("DISPLAY", "NVIDIA GeForce RTX 2060", "NVIDIA", "32.0.16.1656", "20260819000000.******+***"))
            .AddWmi("Win32_PnPSignedDriver", Driver("NET", "WAN Miniport (IP)", "Microsoft", "10.0.1", "20060621000000.******+***"))
            .AddWmi("Win32_PnPSignedDriver", Driver("SYSTEM", "AMD IOMMU Device", "Advanced Micro Devices, Inc.", "1.2.0.43", "20190203000000.******+***"));

        var finding = Assert.Single(Run(new OldDriversCheck(), probe));

        Assert.Equal(StabilitySeverity.Low, finding.Severity);
        var evidence = Assert.Single(finding.Evidence);
        Assert.Contains("AMD Radeon(TM) Graphics", evidence);
        Assert.Contains("06/12/2022", evidence);
    }

    [Fact]
    public void OldBios_OlderThan2Years_IsReported()
    {
        var probe = new FakeStabilityProbe()
            .AddWmi("Win32_BIOS", new() { ["SMBIOSBIOSVersion"] = "FA506IV.320", ["Manufacturer"] = "American Megatrends Inc.", ["ReleaseDate"] = "20220531000000.000000+000" });

        var finding = Assert.Single(Run(new OldBiosCheck(), probe));

        Assert.Contains(finding.Evidence, e => e.Contains("FA506IV.320") && e.Contains("31/05/2022"));
    }

    [Fact]
    public void OldBios_Recent_ReportsNothing()
    {
        var probe = new FakeStabilityProbe()
            .AddWmi("Win32_BIOS", new() { ["SMBIOSBIOSVersion"] = "X.400", ["ReleaseDate"] = "20260101000000.000000+000" });

        Assert.Empty(Run(new OldBiosCheck(), probe));
    }

    [Fact]
    public void ModdedWindows_KnownModInRegisteredOwner_IsReported()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, CurrentVersion, "RegisteredOwner", "WinterOS Rev16")
            .SetRegistry(RegistryHive.LocalMachine, CurrentVersion, "RegisteredOrganization", "WinterOS Project");

        var finding = Assert.Single(Run(new ModdedWindowsCheck(), probe));

        Assert.Equal(StabilitySeverity.Medium, finding.Severity);
        Assert.Contains("WinterOS", finding.Title);
    }

    [Fact]
    public void ModdedWindows_RegularOwner_ReportsNothing()
    {
        var probe = new FakeStabilityProbe()
            .SetRegistry(RegistryHive.LocalMachine, CurrentVersion, "RegisteredOwner", "Zukuth")
            .SetRegistry(RegistryHive.LocalMachine, CurrentVersion, "RegisteredOrganization", "");

        Assert.Empty(Run(new ModdedWindowsCheck(), probe));
    }

    [Fact]
    public void RiskyKernelDrivers_ListsKnownRunningDrivers()
    {
        var probe = new FakeStabilityProbe()
            .AddWmi("Win32_SystemDriver", new() { ["Name"] = "vgk", ["State"] = "Running" })
            .AddWmi("Win32_SystemDriver", new() { ["Name"] = "AMDRyzenMasterDriverV20", ["State"] = "Running" })
            .AddWmi("Win32_SystemDriver", new() { ["Name"] = "RTCore64", ["State"] = "Stopped" })
            .AddWmi("Win32_SystemDriver", new() { ["Name"] = "nvlddmkm", ["State"] = "Running" });

        var finding = Assert.Single(Run(new RiskyKernelDriversCheck(), probe));

        Assert.Equal(2, finding.Evidence.Count);
        Assert.Contains(finding.Evidence, e => e.Contains("vgk") && e.Contains("Riot Vanguard"));
        Assert.Contains(finding.Evidence, e => e.Contains("AMDRyzenMasterDriverV20"));
    }

    private static Dictionary<string, object?> Ram(string bank, string vendor, string part, int gb, int speed) => new()
    {
        ["BankLabel"] = bank,
        ["Manufacturer"] = vendor,
        ["PartNumber"] = part,
        ["Capacity"] = (ulong)gb * 1024 * 1024 * 1024,
        ["Speed"] = (uint)speed,
    };

    private static Dictionary<string, object?> Driver(string cls, string name, string provider, string version, string date) => new()
    {
        ["DeviceClass"] = cls,
        ["DeviceName"] = name,
        ["DriverProviderName"] = provider,
        ["Manufacturer"] = provider,
        ["DriverVersion"] = version,
        ["DriverDate"] = date,
    };
}
