using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.App.Views;
using NexaTweaks.Core.Stability;

namespace NexaTweaks.App.ViewModels;

public partial class StabilityViewModel : ObservableObject
{
    private readonly StabilityAnalyzer _analyzer = new();
    private readonly IStabilityProbe _probe = new WindowsStabilityProbe();

    public ObservableCollection<StabilityFindingViewModel> Findings { get; } = new();

    [ObservableProperty] private bool isAnalyzing;
    [ObservableProperty] private bool hasAnalyzed;
    [ObservableProperty] private string? summary;

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        IsAnalyzing = true;

        IReadOnlyList<StabilityFinding> findings;
        using (BusyService.Instance.Begin("Analizando estabilidad...\nVisor de eventos, drivers y hardware"))
        {
            findings = await Task.Run(() => _analyzer.Analyze(_probe, DateTime.Now));
        }

        Findings.Clear();
        foreach (var finding in findings)
            Findings.Add(new StabilityFindingViewModel(finding));

        Summary = BuildSummary(findings);
        HasAnalyzed = true;
        IsAnalyzing = false;
    }

    [RelayCommand]
    private async Task FixAsync(StabilityFindingViewModel card)
    {
        var fix = card.Finding.Fix;
        if (fix is null) return;

        var proceed = ConfirmDialog.Ask(
            $"Arreglar: {fix.Name}",
            $"{fix.Description}\n\nSe guarda una copia del valor actual: puedes deshacerlo desde Backup.",
            fix.Risk);
        if (!proceed) return;

        card.IsBusy = true;
        var (result, _) = await Task.Run(() =>
            AppServices.Engine.ApplyOneWithEntry(fix, $"Estabilidad: {fix.Name}"));
        card.IsBusy = false;

        if (!result.Success)
        {
            card.StatusMessage = $"No se pudo aplicar: {result.Error}";
            return;
        }

        card.IsFixed = true;
        card.StatusMessage = fix.RequiresRestart
            ? "Arreglado. Reinicia para que se aplique; puedes deshacerlo desde Backup."
            : "Arreglado. Puedes deshacerlo desde Backup.";
        if (fix.RequiresRestart)
            PendingRestartService.Instance.MarkNeeded(fix.Name);
    }

    private static string BuildSummary(IReadOnlyList<StabilityFinding> findings)
    {
        if (findings.Count == 0)
            return "No se encontraron problemas de estabilidad.";

        int Count(StabilitySeverity s) => findings.Count(f => f.Severity == s);
        var fixable = findings.Count(f => f.Fix is not null);
        var text = $"{Count(StabilitySeverity.High)} graves · {Count(StabilitySeverity.Medium)} medios · " +
                   $"{Count(StabilitySeverity.Low)} leves";
        return fixable > 0 ? $"{text} — {fixable} se pueden arreglar desde aquí" : text;
    }
}
