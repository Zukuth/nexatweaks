using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.App.Views;
using NexaTweaks.Core;
using NexaTweaks.Core.Diagnostics;
using NexaTweaks.Core.Engine;
using NexaTweaks.Core.Presets;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.App.ViewModels;

public sealed record PresetCategoryRow(string Category, int Count);

public partial class PresetCardViewModel : ObservableObject
{
    public Preset Preset { get; }
    public string Name => Preset.Name;
    public string Description => Preset.Description;
    public int Count => Preset.Tweaks.Count;
    public string CountText => $"{Count} ajustes";

    /// <summary>Drives the card colour: verde el conservador, azul el intermedio, rojo el agresivo.</summary>
    public string Tone => Preset.Id switch
    {
        "preset.seguro" => "Safe",
        "preset.medio" => "Medium",
        _ => "Extreme",
    };

    /// <summary>A few names so the card shows what it actually does, not just a number.</summary>
    public IReadOnlyList<string> Highlights { get; }
    public IReadOnlyList<PresetCategoryRow> Categories { get; }

    [ObservableProperty] private bool isSelected;

    public PresetCardViewModel(Preset preset)
    {
        Preset = preset;
        Highlights = preset.Tweaks.Select(t => t.Name).Take(18).ToList();
        Categories = preset.CountsByCategory()
            .OrderByDescending(kv => kv.Value)
            .Select(kv => new PresetCategoryRow(Label(kv.Key), kv.Value))
            .ToList();
    }

    private static string Label(TweakCategory category) => category switch
    {
        TweakCategory.Windows => "Windows",
        TweakCategory.Network => "Red",
        TweakCategory.Input => "Input",
        TweakCategory.Gpu => "GPU",
        TweakCategory.Cleanup => "Limpieza",
        TweakCategory.Services => "Servicios",
        TweakCategory.Privacy => "Privacidad",
        TweakCategory.Interface => "Interfaz",
        _ => category.ToString(),
    };
}

public partial class PresetsViewModel : ObservableObject
{
    public ObservableCollection<PresetCardViewModel> Presets { get; } = new();

    [ObservableProperty] private PresetCardViewModel? selected;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    public PresetsViewModel()
    {
        foreach (var preset in PresetCatalog.All) Presets.Add(new PresetCardViewModel(preset));
        Select(Presets[0]);
    }

    [RelayCommand]
    private void Select(PresetCardViewModel card)
    {
        foreach (var preset in Presets) preset.IsSelected = ReferenceEquals(preset, card);
        Selected = card;
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (Selected is not { } card || IsBusy) return;

        var proceed = ConfirmDialog.Ask(
            $"Aplicar el preset {card.Name}",
            $"{card.Description}\n\nSe aplicarán hasta {card.Count} ajustes. Todos quedan guardados en Backup y se pueden revertir.",
            RiskLevel.Advanced);
        if (!proceed) return;

        IsBusy = true;
        StatusMessage = null;
        ActivityLog.Instance.Info($"Preset {card.Name}: revisando {card.Count} ajustes...");

        var results = await Task.Run(() =>
        {
            var pending = card.Preset.Tweaks.Where(Available).Where(NotYetApplied).ToList();
            if (pending.Count == 0) return new List<TweakResult>();

            ActivityLog.Instance.Info($"Preset {card.Name}: aplicando {pending.Count} pendientes...");
            RestorePointGuard.EnsureBeforeChanges($"preset {card.Name}");
            var (_, applied) = AppServices.Engine.ApplyMany(pending, $"Preset {card.Name}");
            return applied.ToList();
        });

        var ok = results.Count(r => r.Success);
        var failed = results.Count(r => !r.Success);

        foreach (var result in results.Where(r => !r.Success))
            ActivityLog.Instance.Fail($"Falló «{result.Tweak.Name}»: {result.Error}");
        foreach (var result in results.Where(r => r.Success && r.Tweak.RequiresRestart))
            PendingRestartService.Instance.MarkNeeded(result.Tweak.Name);

        StatusMessage = results.Count == 0
            ? "Este preset ya estaba aplicado por completo."
            : failed == 0
                ? $"{ok} ajustes aplicados. Puedes revertirlos desde Backup."
                : $"{ok} aplicados y {failed} con errores (mira el registro).";
        ActivityLog.Instance.Add(failed == 0 ? ActivityLevel.Ok : ActivityLevel.Warn,
            $"Preset {card.Name}: {StatusMessage}");
        IsBusy = false;
    }

    /// <summary>Skips what this machine doesn't have (a missing service, software not installed):
    /// applying those only produces errors in the log.</summary>
    private static bool Available(ITweak tweak)
    {
        try { return tweak.IsAvailable(); }
        catch { return true; }
    }

    /// <summary>A tweak whose current state can't be read is treated as pending: applying it again
    /// is harmless, skipping it silently is not.</summary>
    private static bool NotYetApplied(ITweak tweak)
    {
        try { return !tweak.IsApplied(); }
        catch (Exception ex)
        {
            AppLog.Error($"Preset IsApplied {tweak.Id}", ex);
            return true;
        }
    }
}
