using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.App.Views;
using NexaTweaks.Core;
using NexaTweaks.Core.Cleanup;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App.ViewModels;

public sealed record InstalledAppRowViewModel(string DisplayName, string PublisherText, string VersionText, string UninstallString);

public partial class AppsViewModel : ObservableObject
{
    private readonly List<InstalledAppRowViewModel> _allApps = new();

    public ObservableCollection<InstalledAppRowViewModel> FilteredApps { get; } = new();

    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private string? countText;

    public AppsViewModel() => _ = LoadAsync();

    private async Task LoadAsync()
    {
        IsLoading = true;
        using var busy = BusyService.Instance.Begin("Buscando programas instalados...");

        var apps = await Task.Run(InstalledAppsDiscovery.Discover);

        _allApps.Clear();
        _allApps.AddRange(apps.Select(a => new InstalledAppRowViewModel(
            a.DisplayName, a.Publisher ?? "Editor desconocido", a.Version ?? "", a.UninstallString)));

        ApplyFilter();
        IsLoading = false;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredApps.Clear();
        var query = string.IsNullOrWhiteSpace(SearchText)
            ? _allApps
            : _allApps.Where(a => a.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var app in query) FilteredApps.Add(app);
        CountText = $"{_allApps.Count} programa(s) instalados";
    }

    [RelayCommand]
    private void Uninstall(InstalledAppRowViewModel row)
    {
        var proceed = ConfirmDialog.Ask(
            $"Desinstalar {row.DisplayName}",
            "Se abrirá el desinstalador oficial de esta aplicación - seguí sus pasos para completarlo. Nexa Tweaks no borra nada por sí mismo.",
            RiskLevel.Advanced);
        if (!proceed) return;

        try
        {
            Process.Start(new ProcessStartInfo("cmd.exe", $"/c {row.UninstallString}") { UseShellExecute = false });
            StatusMessage = $"Abriendo el desinstalador de \"{row.DisplayName}\"...";
        }
        catch (Exception ex)
        {
            AppLog.Error("Uninstall app", ex);
            StatusMessage = $"No se pudo iniciar el desinstalador: {ex.Message}";
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();
}
