namespace NexaTweaks.Core.Diagnostics;

public enum ActivityLevel
{
    Info,
    Ok,
    Warn,
    Fail,
}

public sealed record ActivityEntry(DateTime Timestamp, ActivityLevel Level, string Message)
{
    public string Line => $"[{Timestamp:HH:mm:ss}] {Message}";
}

/// <summary>
/// In-memory activity log shown in the app's bottom panel: every action (apply, revert, repair,
/// cleanup) records what it did, so the user can see the app's work without opening a log file.
/// Capped so a long session can't grow without bound; <see cref="AppLog"/> stays the on-disk log.
/// </summary>
public sealed class ActivityLog
{
    public const int DefaultCapacity = 500;

    public static ActivityLog Instance { get; } = new();

    private readonly object _lock = new();
    private readonly Queue<ActivityEntry> _entries;
    private readonly int _capacity;

    public ActivityLog(int capacity = DefaultCapacity)
    {
        _capacity = capacity > 0 ? capacity : DefaultCapacity;
        _entries = new Queue<ActivityEntry>(_capacity);
    }

    /// <summary>Raised on the thread that logged the entry - subscribers on the UI must marshal.</summary>
    public event Action<ActivityEntry>? EntryAdded;

    public event Action? Cleared;

    public IReadOnlyList<ActivityEntry> Entries
    {
        get { lock (_lock) { return _entries.ToArray(); } }
    }

    public void Info(string message) => Add(ActivityLevel.Info, message);
    public void Ok(string message) => Add(ActivityLevel.Ok, message);
    public void Warn(string message) => Add(ActivityLevel.Warn, message);
    public void Fail(string message) => Add(ActivityLevel.Fail, message);

    public void Add(ActivityLevel level, string message)
    {
        var entry = new ActivityEntry(DateTime.Now, level, message);
        lock (_lock)
        {
            if (_entries.Count == _capacity) _entries.Dequeue();
            _entries.Enqueue(entry);
        }
        EntryAdded?.Invoke(entry);
    }

    public void Clear()
    {
        lock (_lock) { _entries.Clear(); }
        Cleared?.Invoke();
    }

    public string ExportText() => string.Join(Environment.NewLine, Entries.Select(e => e.Line));
}
