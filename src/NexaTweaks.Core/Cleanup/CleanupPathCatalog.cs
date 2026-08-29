namespace NexaTweaks.Core.Cleanup;

/// <summary>
/// Safe, recreatable cache locations identified during a static audit of CarpiFPS.
/// These paths deliberately exclude user documents, game installs, WindowsApps and browser data.
/// </summary>
public static class CleanupPathCatalog
{
    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static string AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static IReadOnlyList<string> ShaderCaches() => new[]
    {
        Path.Combine(LocalAppData, "D3DSCache"),
        Path.Combine(LocalAppData, "NVIDIA", "DXCache"),
        Path.Combine(LocalAppData, "NVIDIA", "GLCache"),
        Path.Combine(LocalAppData, "AMD", "DxCache"),
        Path.Combine(LocalAppData, "AMD", "GLCache"),
    };

    public static IReadOnlyList<string> DiscordCaches() => new[]
    {
        Path.Combine(AppData, "discord", "Cache"),
        Path.Combine(AppData, "discord", "Code Cache"),
        Path.Combine(AppData, "discord", "GPUCache"),
        Path.Combine(AppData, "discord", "Crashpad", "reports"),
    };

    public static IReadOnlyList<string> LauncherCaches() => new[]
    {
        Path.Combine(LocalAppData, "Steam", "htmlcache"),
        Path.Combine(LocalAppData, "EpicGamesLauncher", "Saved", "webcache"),
    };

    public static IReadOnlyList<string> CrashReports() => new[]
    {
        Path.Combine(LocalAppData, "CrashDumps"),
        Path.Combine(LocalAppData, "Microsoft", "Windows", "WER", "ReportArchive"),
        Path.Combine(LocalAppData, "Microsoft", "Windows", "WER", "ReportQueue"),
    };

    public static IReadOnlyList<string> DeveloperCaches() => new[]
    {
        Path.Combine(LocalAppData, "npm-cache", "_cacache"),
        Path.Combine(LocalAppData, "Yarn", "Cache"),
        Path.Combine(LocalAppData, "NuGet", "v3-cache"),
    };

    public static IReadOnlyList<string> All() => ShaderCaches()
        .Concat(DiscordCaches())
        .Concat(LauncherCaches())
        .Concat(CrashReports())
        .Concat(DeveloperCaches())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
}
