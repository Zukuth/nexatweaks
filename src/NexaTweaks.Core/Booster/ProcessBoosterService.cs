using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NexaTweaks.Core.Booster;

/// <summary>
/// Raises the target game process to High priority and drops a curated list of non-essential
/// background processes to Idle priority while boosting, restoring both on Stop. Optionally
/// pins the Windows timer resolution to its minimum (~0.5-1ms) for the duration of the boost,
/// the same trick dedicated "timer resolution" tools use for smoother frame pacing.
/// </summary>
public sealed class ProcessBoosterService
{
    private static readonly HashSet<string> BackgroundTrimTargets = new(StringComparer.OrdinalIgnoreCase)
    {
        "OneDrive", "Teams", "Spotify", "Discord", "Skype", "EpicGamesLauncher",
        "GalaxyClient", "Origin", "GoogleDriveFS", "Dropbox",
    };

    private readonly Dictionary<int, ProcessPriorityClass> _restorePriorities = new();
    private int? _boostedPid;
    private bool _timerResolutionActive;

    public bool IsBoosting { get; private set; }
    public string? BoostedProcessName { get; private set; }

    public bool Start(string processName, bool boostTimerResolution = false)
    {
        using var target = Process.GetProcessesByName(processName).FirstOrDefault();
        if (target is null) return false;

        try
        {
            target.PriorityClass = ProcessPriorityClass.High;
            _boostedPid = target.Id;
            BoostedProcessName = processName;
        }
        catch
        {
            return false;
        }

        _restorePriorities.Clear();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                if (!BackgroundTrimTargets.Contains(process.ProcessName)) continue;
                try
                {
                    _restorePriorities[process.Id] = process.PriorityClass;
                    process.PriorityClass = ProcessPriorityClass.Idle;
                }
                catch
                {
                    // process may have exited or be protected - skip it
                }
            }
        }

        if (boostTimerResolution)
            _timerResolutionActive = TimerResolution.TrySetHighResolution();

        IsBoosting = true;
        return true;
    }

    public void Stop()
    {
        if (_boostedPid is int pid)
        {
            try
            {
                using var target = Process.GetProcessById(pid);
                target.PriorityClass = ProcessPriorityClass.Normal;
            }
            catch
            {
                // process already exited
            }
        }

        foreach (var (processId, priority) in _restorePriorities)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                process.PriorityClass = priority;
            }
            catch
            {
                // process already exited
            }
        }

        if (_timerResolutionActive)
        {
            TimerResolution.Restore();
            _timerResolutionActive = false;
        }

        _restorePriorities.Clear();
        _boostedPid = null;
        BoostedProcessName = null;
        IsBoosting = false;
    }

    /// <summary>Returns the first profile whose process is currently running, or null if none are.</summary>
    public static GameProfile? FindRunningProfile(IEnumerable<GameProfile> profiles)
    {
        foreach (var profile in profiles)
        {
            var processes = Process.GetProcessesByName(profile.ProcessName);
            try
            {
                if (processes.Length > 0) return profile;
            }
            finally
            {
                foreach (var p in processes) p.Dispose();
            }
        }
        return null;
    }
}

/// <summary>Thin wrapper around ntdll's NtSetTimerResolution - the same low-level call timer-resolution utilities use.</summary>
internal static class TimerResolution
{
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int NtSetTimerResolution(uint desiredResolution, bool setResolution, out uint currentResolution);

    public static bool TrySetHighResolution()
    {
        try
        {
            // 5000 * 100ns = 0.5ms, the minimum most hardware clocks support.
            return NtSetTimerResolution(5000, true, out _) == 0;
        }
        catch
        {
            return false;
        }
    }

    public static void Restore()
    {
        try { NtSetTimerResolution(5000, false, out _); }
        catch { /* best effort */ }
    }
}
