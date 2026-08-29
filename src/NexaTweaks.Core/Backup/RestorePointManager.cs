using System.Management;

namespace NexaTweaks.Core.Backup;

public static class RestorePointManager
{
    public static bool TryCreateRestorePoint(string description, out string? error)
    {
        try
        {
            using var managementClass = new ManagementClass("root\\default", "SystemRestore", null);
            using var inParams = managementClass.GetMethodParameters("CreateRestorePoint");
            inParams["Description"] = description;
            inParams["RestorePointType"] = 12; // MODIFY_SETTINGS
            inParams["EventType"] = 100; // BEGIN_SYSTEM_CHANGE
            using var result = managementClass.InvokeMethod("CreateRestorePoint", inParams, null);
            var returnValue = result is not null ? Convert.ToUInt32(result["ReturnValue"]) : 1u;

            if (returnValue != 0)
            {
                error = $"System Restore devolvió el código {returnValue}. Puede estar deshabilitado en este equipo.";
                return false;
            }

            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
