using System.IO.Compression;

namespace NexaTweaks.Core.Diagnostics;

/// <summary>Bundles system info and recent error logs into one zip on the Desktop, for support requests.</summary>
public static class SupportBundleExporter
{
    public static string Export()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var zipPath = Path.Combine(desktop, $"NexaTweaks-diagnostico-{DateTime.Now:yyyyMMdd-HHmmss}.zip");

        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);

        var info = SystemInfoService.Get();
        var infoText =
            "Nexa Tweaks - Diagnostico\r\n" +
            $"Generado: {DateTime.Now}\r\n\r\n" +
            $"Sistema operativo: {info.OsName} ({info.OsVersion})\r\n" +
            $"CPU: {info.CpuName} ({info.CpuCores} nucleos / {info.CpuLogicalProcessors} hilos)\r\n" +
            $"GPU: {info.GpuName}\r\n" +
            $"RAM total: {info.RamTotalGb} GB\r\n" +
            $"Placa madre: {info.MotherboardName}\r\n" +
            $"Arquitectura: {info.SystemType}\r\n";

        var infoEntry = zip.CreateEntry("sistema.txt");
        using (var writer = new StreamWriter(infoEntry.Open()))
            writer.Write(infoText);

        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NexaTweaks", "logs");
        if (Directory.Exists(logDir))
        {
            foreach (var file in Directory.GetFiles(logDir, "*.log"))
                zip.CreateEntryFromFile(file, Path.Combine("logs", Path.GetFileName(file)));
        }

        return zipPath;
    }
}
