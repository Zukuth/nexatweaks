namespace NexaTweaks.Core.Cleanup;

/// <summary>Resolves each browser's cache folder(s) - cache only, never cookies/history/passwords, so clearing it never logs anyone out.</summary>
public static class BrowserCachePaths
{
    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static string AppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public static List<string> Chrome() => new()
    {
        Path.Combine(LocalAppData, "Google", "Chrome", "User Data", "Default", "Cache"),
        Path.Combine(LocalAppData, "Google", "Chrome", "User Data", "Default", "Code Cache"),
    };

    public static List<string> Edge() => new()
    {
        Path.Combine(LocalAppData, "Microsoft", "Edge", "User Data", "Default", "Cache"),
        Path.Combine(LocalAppData, "Microsoft", "Edge", "User Data", "Default", "Code Cache"),
    };

    public static List<string> Firefox()
    {
        var profilesRoot = Path.Combine(AppData, "Mozilla", "Firefox", "Profiles");
        if (!Directory.Exists(profilesRoot)) return new List<string>();

        try
        {
            return Directory.GetDirectories(profilesRoot)
                .Select(p => Path.Combine(p, "cache2"))
                .Where(Directory.Exists)
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}
