using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public string AdminStatus { get; }
    public string BackupFolder { get; }

    public string CpuText { get; }
    public string GpuText { get; }
    public string RamText { get; }
    public string OsText { get; }
    public string MotherboardText { get; }

    [ObservableProperty] private string? exportStatus;

    public SettingsViewModel()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        AdminStatus = principal.IsInRole(WindowsBuiltInRole.Administrator)
            ? "Ejecutándose como administrador. Todos los tweaks están disponibles."
            : "No se está ejecutando como administrador. Algunos tweaks pueden fallar al aplicarse.";

        BackupFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NexaTweaks", "Backups");

        var info = SystemInfoService.Get();
        CpuText = $"{info.CpuName} · {info.CpuCores} núcleos / {info.CpuLogicalProcessors} hilos";
        GpuText = info.GpuName;
        RamText = $"{info.RamTotalGb:F1} GB";
        OsText = $"{info.OsName} · {info.OsVersion} · {info.SystemType}";
        MotherboardText = info.MotherboardName;
    }

    [RelayCommand]
    private void OpenBackupFolder()
    {
        Directory.CreateDirectory(BackupFolder);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{BackupFolder}\"") { UseShellExecute = true });
    }

    [RelayCommand]
    private async Task ExportSupportBundleAsync()
    {
        ExportStatus = "Generando paquete de diagnóstico...";
        try
        {
            var path = await Task.Run(SupportBundleExporter.Export);
            ExportStatus = $"Listo: {path}";
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            AppLog.Error("Export support bundle", ex);
            ExportStatus = $"No se pudo generar el diagnóstico: {ex.Message}";
        }
    }
}
