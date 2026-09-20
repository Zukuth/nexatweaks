using NexaTweaks.App.ViewModels;
using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App.Services;

/// <summary>
/// Creates a Windows restore point before the first batch of changes of the session, when the
/// user left the switch on. Windows itself rate-limits restore points, so one per session keeps
/// the app responsive instead of stalling on every single toggle.
/// </summary>
public static class RestorePointGuard
{
    private static readonly object Lock = new();
    private static bool _createdThisSession;

    public static void EnsureBeforeChanges(string reason)
    {
        if (!ActivityLogViewModel.Instance.CreateRestorePoint) return;

        lock (Lock)
        {
            if (_createdThisSession) return;
            _createdThisSession = true;
        }

        ActivityLog.Instance.Info($"Creando punto de restauración antes de: {reason}...");
        var ok = RestorePointManager.TryCreateRestorePoint($"NexaTweaks - {reason}", out var error);
        if (ok)
        {
            ActivityLog.Instance.Ok("Punto de restauración creado.");
        }
        else
        {
            ActivityLog.Instance.Warn($"No se pudo crear el punto de restauración: {error}");
            lock (Lock) { _createdThisSession = false; }
        }
    }
}
