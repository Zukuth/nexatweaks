namespace NexaTweaks.Core.Cleanup;

public sealed record LargeFileEntry(string Path, long SizeBytes);

/// <summary>
/// Finds the largest files under a root folder - the same "visual disk analyzer" idea Hone
/// advertises, kept simple as a sorted top-N scan instead of a full treemap.
/// </summary>
public static class DiskSpaceAnalyzer
{
    public static List<LargeFileEntry> FindLargestFiles(string rootPath, int topN, CancellationToken ct = default)
    {
        var results = new List<LargeFileEntry>();
        if (!Directory.Exists(rootPath)) return results;

        var stack = new Stack<string>();
        stack.Push(rootPath);

        while (stack.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var dir = stack.Pop();

            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(dir); }
            catch { continue; }

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    results.Add(new LargeFileEntry(file, info.Length));
                }
                catch
                {
                    // inaccessible/locked file - skip it
                }
            }

            IEnumerable<string> subDirs;
            try { subDirs = Directory.EnumerateDirectories(dir); }
            catch { continue; }

            foreach (var subDir in subDirs)
                stack.Push(subDir);
        }

        return results.OrderByDescending(f => f.SizeBytes).Take(topN).ToList();
    }
}
