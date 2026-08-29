using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.App.Views;
using NexaTweaks.Core;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App.ViewModels;

public partial class AutorunRowViewModel : ObservableObject
{
    public AutorunEntry Entry { get; }
    public string Source => Entry.Source;
    public string Name => Entry.Name;
    public string Command => Entry.Command;
    public bool IsMicrosoftSigned => Entry.IsMicrosoftSigned;
    public RiskLevel Risk => Entry.DisableTweak.Risk;

    [ObservableProperty] private bool isDisabled;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? errorMessage;

    public AutorunRowViewModel(AutorunEntry entry) => Entry = entry;
}

public partial class AutorunViewModel : ObservableObject
{
    private readonly List<AutorunRowViewModel> _allEntries = new();

    public ObservableCollection<AutorunRowViewModel> FilteredEntries { get; } = new();

    [ObservableProperty] private bool isScanning;
    [ObservableProperty] private bool hasScanned;
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private bool showMicrosoftEntries;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private int totalCount;
    [ObservableProperty] private int thirdPartyCount;

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsScanning = true;
        using var busy = BusyService.Instance.Begin("Escaneando programas, tareas y servicios que arrancan con Windows...");

        var entries = await Task.Run(AutorunScanner.Scan);

        _allEntries.Clear();
        _allEntries.AddRange(entries.Select(e => new AutorunRowViewModel(e)));

        TotalCount = _allEntries.Count;
        ThirdPartyCount = _allEntries.Count(e => !e.IsMicrosoftSigned);

        ApplyFilter();
        HasScanned = true;
        IsScanning = false;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnShowMicrosoftEntriesChanged(bool value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredEntries.Clear();

        IEnumerable<AutorunRowViewModel> query = _allEntries;
        if (!ShowMicrosoftEntries) query = query.Where(e => !e.IsMicrosoftSigned);
        if (!string.IsNullOrWhiteSpace(SearchText))
            query = query.Where(e => e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                      e.Command.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var entry in query) FilteredEntries.Add(entry);
    }

    [RelayCommand]
    private async Task DisableAsync(AutorunRowViewModel row)
    {
        var proceed = ConfirmDialog.Ask(
            $"Deshabilitar: {row.Name}",
            $"Origen: {row.Source}\nComando: {row.Command}\n\nEsto lo detiene para que no vuelva a arrancar solo. Podés revertirlo desde la pestaña Backup.",
            row.Risk);
        if (!proceed) return;

        row.IsBusy = true;
        row.ErrorMessage = null;

        var result = await Task.Run(() =>
            AppServices.Engine.ApplyOne(row.Entry.DisableTweak, $"Autorun - {row.Name}"));

        row.IsDisabled = result.Success;
        row.ErrorMessage = result.Success ? null : result.Error;
        row.IsBusy = false;
    }
}
