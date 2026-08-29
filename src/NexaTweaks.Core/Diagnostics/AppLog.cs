namespace NexaTweaks.Core.Diagnostics;

/// <summary>Minimal file logger for uncaught errors - keeps the app diagnosable without an external logging package.</summary>
public static class AppLog
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NexaTweaks", "logs");

    private static readonly object Lock = new();

    public static void Error(string context, Exception ex)
    {
        try
        {
            lock (Lock)
            {
                Directory.CreateDirectory(LogDir);
                var path = Path.Combine(LogDir, $"error-{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(path,
                    $"[{DateTime.Now:HH:mm:ss}] {context}{Environment.NewLine}{ex}{Environment.NewLine}{new string('-', 60)}{Environment.NewLine}");
            }
        }
        catch
        {
            // logging must never itself throw
        }
    }
}
