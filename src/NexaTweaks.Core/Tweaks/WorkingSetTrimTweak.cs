using System.Diagnostics;
using System.Runtime.InteropServices;
using NexaTweaks.Core.Backup;

namespace NexaTweaks.Core.Tweaks;

/// <summary>
/// Trims the working set of every accessible process, pushing idle pages out of physical RAM
/// back to the standby list. Real, measurable, and harmless - the OS re-pages memory back in
/// as needed. Not reversible in a meaningful sense (there's nothing to undo).
/// </summary>
public sealed class WorkingSetTrimTweak : TweakBase
{
    public override bool IsReversible => false;

    public int LastProcessesTrimmed { get; private set; }

    public override bool IsApplied() => false;

    public override BackupEntry Apply()
    {
        var trimmed = 0;
        foreach (var process in Process.GetProcesses())
        {
            try
            {
                using (process)
                {
                    if (EmptyWorkingSet(process.Handle))
                        trimmed++;
                }
            }
            catch
            {
                // access denied for protected/system processes - expected, skip it
            }
        }
        LastProcessesTrimmed = trimmed;
        return BackupEntry.CreateEmpty(this);
    }

    public override void Revert(BackupEntry entry)
    {
        // no-op: nothing to undo, the OS naturally re-pages memory as needed
    }

    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(IntPtr hProcess);
}
