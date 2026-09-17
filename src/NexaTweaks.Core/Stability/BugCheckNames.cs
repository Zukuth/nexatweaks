namespace NexaTweaks.Core.Stability;

/// <summary>Symbolic names of the stop codes most often seen on consumer PCs
/// (see learn.microsoft.com/windows-hardware/drivers/debugger/bug-check-code-reference2).</summary>
internal static class BugCheckNames
{
    private static readonly Dictionary<uint, string> Names = new()
    {
        [0x0A] = "IRQL_NOT_LESS_OR_EQUAL",
        [0x19] = "BAD_POOL_HEADER",
        [0x1A] = "MEMORY_MANAGEMENT",
        [0x1E] = "KMODE_EXCEPTION_NOT_HANDLED",
        [0x3B] = "SYSTEM_SERVICE_EXCEPTION",
        [0x50] = "PAGE_FAULT_IN_NONPAGED_AREA",
        [0x7A] = "KERNEL_DATA_INPAGE_ERROR",
        [0x7E] = "SYSTEM_THREAD_EXCEPTION_NOT_HANDLED",
        [0x7F] = "UNEXPECTED_KERNEL_MODE_TRAP",
        [0x9F] = "DRIVER_POWER_STATE_FAILURE",
        [0xC2] = "BAD_POOL_CALLER",
        [0xD1] = "DRIVER_IRQL_NOT_LESS_OR_EQUAL",
        [0xEF] = "CRITICAL_PROCESS_DIED",
        [0x101] = "CLOCK_WATCHDOG_TIMEOUT",
        [0x116] = "VIDEO_TDR_FAILURE",
        [0x117] = "VIDEO_TDR_TIMEOUT_DETECTED",
        [0x124] = "WHEA_UNCORRECTABLE_ERROR",
        [0x12B] = "FAULTY_HARDWARE_CORRUPTED_PAGE",
        [0x133] = "DPC_WATCHDOG_VIOLATION",
        [0x139] = "KERNEL_SECURITY_CHECK_FAILURE",
        [0x154] = "UNEXPECTED_STORE_EXCEPTION",
        [0x1CA] = "SYNTHETIC_WATCHDOG_TIMEOUT",
    };

    public static string Get(uint? code) =>
        code is not null && Names.TryGetValue(code.Value, out var name) ? name : "(código sin nombre conocido)";
}
