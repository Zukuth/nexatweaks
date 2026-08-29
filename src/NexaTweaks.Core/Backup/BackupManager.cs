using System.Text.Json;

namespace NexaTweaks.Core.Backup;

public sealed class BackupManager
{
    private readonly string _backupDir;

    public BackupManager(string? rootOverride = null)
    {
        _backupDir = rootOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "NexaTweaks", "Backups");
        Directory.CreateDirectory(_backupDir);
    }

    public BackupSnapshot CreateSnapshot(string label, IEnumerable<BackupEntry> entries)
    {
        var snapshot = new BackupSnapshot { Label = label, Entries = entries.ToList() };
        Save(snapshot);
        return snapshot;
    }

    public void Save(BackupSnapshot snapshot)
    {
        var path = Path.Combine(_backupDir, $"{snapshot.Timestamp:yyyyMMdd-HHmmss}_{snapshot.Id:N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Delete(BackupSnapshot snapshot)
    {
        var file = Directory.GetFiles(_backupDir, $"*{snapshot.Id:N}.json").FirstOrDefault();
        if (file is not null) File.Delete(file);
    }

    public IReadOnlyList<BackupSnapshot> LoadAll()
    {
        if (!Directory.Exists(_backupDir)) return Array.Empty<BackupSnapshot>();

        var result = new List<BackupSnapshot>();
        foreach (var file in Directory.GetFiles(_backupDir, "*.json"))
        {
            try
            {
                var snap = JsonSerializer.Deserialize<BackupSnapshot>(File.ReadAllText(file));
                if (snap is not null) result.Add(snap);
            }
            catch
            {
                // ignore corrupt snapshot files
            }
        }
        return result.OrderByDescending(s => s.Timestamp).ToList();
    }
}
