using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Stability;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.App.ViewModels;

public partial class SnapshotCardViewModel : ObservableObject
{
    public BackupSnapshot Snapshot { get; }
    public string Label => Snapshot.Label;
    public string Timestamp => Snapshot.Timestamp.LocalDateTime.ToString("dd/MM/yyyy HH:mm:ss");
    public int EntryCount => Snapshot.Entries.Count;
    public string EntrySummary => string.Join(", ", Snapshot.Entries.Select(e => e.TweakName).Distinct().Take(4)) +
                                   (Snapshot.Entries.Count > 4 ? $" y {Snapshot.Entries.Count - 4} más..." : "");

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    public SnapshotCardViewModel(BackupSnapshot snapshot) => Snapshot = snapshot;
}

public partial class BackupViewModel : ObservableObject
{
    public ObservableCollection<SnapshotCardViewModel> Snapshots { get; } = new();

    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;

    public BackupViewModel() => _ = ReloadAsync();

    private async Task ReloadAsync()
    {
        IsBusy = true;
        // Snapshot history only grows over time (one file per applied batch) - reading and
        // deserializing all of it is real disk I/O, so it happens off the UI thread.
        var snapshots = await Task.Run(() => AppServices.BackupManager.LoadAll());

        Snapshots.Clear();
        foreach (var snap in snapshots)
            Snapshots.Add(new SnapshotCardViewModel(snap));
        IsBusy = false;
    }

    [RelayCommand]
    private void CreateRestorePoint()
    {
        var ok = RestorePointManager.TryCreateRestorePoint("NexaTweaks - punto de restauración manual", out var error);
        StatusMessage = ok
            ? "Punto de restauración de Windows creado correctamente."
            : $"No se pudo crear el punto de restauración: {error}";
    }

    [RelayCommand]
    private async Task RestoreAsync(SnapshotCardViewModel card)
    {
        card.IsBusy = true;

        using var busy = BusyService.Instance.Begin($"Restaurando \"{card.Label}\"...");

        var results = await Task.Run(() =>
        {
            var tweaksById = BuildTweaksById();
            return AppServices.Engine.RestoreSnapshot(card.Snapshot, tweaksById);
        });
        var failed = results.Count(r => !r.Success);

        card.StatusMessage = failed == 0
            ? "Restaurado correctamente."
            : $"{failed} elemento(s) no se pudieron restaurar (puede que ya no existan).";
        card.IsBusy = false;

        await ReloadAsync();
    }

    private static Dictionary<string, ITweak> BuildTweaksById()
    {
        var all = TweakCatalog.Windows
            .Concat(TweakCatalog.Network)
            .Concat(TweakCatalog.Input)
            .Concat(TweakCatalog.Gpu)
            .Concat(TweakCatalog.Cleanup)
            .Concat(TweakCatalog.Advanced)
            .Concat(StabilityFixes.All)
            .Concat(StartupItemDiscovery.Discover());

        var dict = new Dictionary<string, ITweak>();
        foreach (var tweak in all) dict[tweak.Id] = tweak;
        return dict;
    }
}
