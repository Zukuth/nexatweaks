using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Cleanup;

namespace NexaTweaks.App.ViewModels;

public sealed record LargeFileRowViewModel(string Path, string SizeText, string FolderPath);

public partial class RegistryIssueRowViewModel : ObservableObject
{
    public RegistryIssue Issue { get; }
    public string Category => Issue.Category;
    public string Description => Issue.Description;

    [ObservableProperty] private bool isFixed;
    [ObservableProperty] private bool isFixing;
    [ObservableProperty] private string? errorMessage;

    public RegistryIssueRowViewModel(RegistryIssue issue) => Issue = issue;
}

public partial class CleanupViewModel : ObservableObject
{
    public TweakCategoryViewModel Tweaks { get; }
    public ObservableCollection<LargeFileRowViewModel> LargeFiles { get; } = new();
    public ObservableCollection<RegistryIssueRowViewModel> RegistryIssues { get; } = new();

    [ObservableProperty] private bool isScanning;
    [ObservableProperty] private string? scanStatus;
    [ObservableProperty] private string scanPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    [ObservableProperty] private bool isScanningRegistry;
    [ObservableProperty] private string? registryScanStatus;
    [ObservableProperty] private bool isFixingAllRegistry;

    public CleanupViewModel()
    {
        Tweaks = new TweakCategoryViewModel(
            "Limpieza", "Temporales, prefetch, RAM en espera y gestor de apps de inicio.",
            TweakCatalog.Cleanup.Concat(StartupItemDiscovery.Discover()).ToList());
    }

    [RelayCommand]
    private async Task ScanDiskAsync()
    {
        IsScanning = true;
        ScanStatus = null;
        LargeFiles.Clear();

        using var busy = BusyService.Instance.Begin($"Buscando los archivos más grandes en\n{ScanPath}...");
        var results = await Task.Run(() => DiskSpaceAnalyzer.FindLargestFiles(ScanPath, 25));

        foreach (var entry in results)
            LargeFiles.Add(new LargeFileRowViewModel(entry.Path, FormatSize(entry.SizeBytes), Path.GetDirectoryName(entry.Path) ?? ScanPath));

        ScanStatus = results.Count == 0
            ? "No se encontraron archivos accesibles en esa ruta."
            : $"{results.Count} archivo(s) más grandes encontrados.";
        IsScanning = false;
    }

    [RelayCommand]
    private void OpenFolder(LargeFileRowViewModel row)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{row.Path}\"") { UseShellExecute = true });
        }
        catch
        {
            // best effort - folder may have been removed since the scan
        }
    }

    [RelayCommand]
    private async Task ScanRegistryAsync()
    {
        IsScanningRegistry = true;
        RegistryScanStatus = null;
        RegistryIssues.Clear();

        using var busy = BusyService.Instance.Begin("Buscando entradas huérfanas en el registro...");
        var issues = await Task.Run(RegistryCleanerScanner.Scan);

        foreach (var issue in issues)
            RegistryIssues.Add(new RegistryIssueRowViewModel(issue));

        RegistryScanStatus = issues.Count == 0
            ? "No se encontraron entradas huérfanas. Tu registro está limpio."
            : $"{issues.Count} entrada(s) huérfana(s) encontradas.";
        IsScanningRegistry = false;
    }

    [RelayCommand]
    private async Task FixRegistryIssueAsync(RegistryIssueRowViewModel row)
    {
        row.IsFixing = true;
        row.ErrorMessage = null;

        var result = await Task.Run(() =>
            AppServices.Engine.ApplyOne(row.Issue.FixTweak, $"Limpieza de registro - {row.Issue.FixTweak.Name}"));

        row.IsFixed = result.Success;
        row.ErrorMessage = result.Success ? null : result.Error;
        row.IsFixing = false;
    }

    [RelayCommand]
    private async Task FixAllRegistryIssuesAsync()
    {
        var pending = RegistryIssues.Where(r => !r.IsFixed).ToList();
        if (pending.Count == 0) return;

        IsFixingAllRegistry = true;
        using var busy = BusyService.Instance.Begin($"Corrigiendo {pending.Count} entrada(s) del registro...");

        foreach (var row in pending)
        {
            row.IsFixing = true;
            var result = await Task.Run(() =>
                AppServices.Engine.ApplyOne(row.Issue.FixTweak, $"Limpieza de registro - {row.Issue.FixTweak.Name}"));
            row.IsFixed = result.Success;
            row.ErrorMessage = result.Success ? null : result.Error;
            row.IsFixing = false;
        }

        RegistryScanStatus = $"{pending.Count(r => r.IsFixed)} de {pending.Count} corregidas.";
        IsFixingAllRegistry = false;
    }

    private static string FormatSize(long bytes)
    {
        double gb = bytes / 1024d / 1024 / 1024;
        if (gb >= 1) return $"{gb:F2} GB";
        double mb = bytes / 1024d / 1024;
        return $"{mb:F1} MB";
    }
}
