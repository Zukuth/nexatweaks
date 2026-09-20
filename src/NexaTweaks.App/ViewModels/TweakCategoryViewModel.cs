using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.App.Views;
using NexaTweaks.Core;
using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Diagnostics;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.App.ViewModels;

public partial class TweakCategoryViewModel : ObservableObject
{
    public string Title { get; }
    public string Subtitle { get; }
    public ObservableCollection<TweakCardViewModel> Cards { get; }

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private bool isBusy;

    public int PendingCount => Cards.Count(c => c.IsPending);

    public TweakCategoryViewModel(string title, string subtitle, IReadOnlyList<ITweak> tweaks)
    {
        Title = title;
        Subtitle = subtitle;

        // Cards render instantly with their declared default - checking real state means a
        // registry/WMI/service query per tweak, which can noticeably stall page navigation if
        // done synchronously here. That work happens in the background right after instead.
        Cards = new ObservableCollection<TweakCardViewModel>(tweaks.Select(t =>
        {
            var card = new TweakCardViewModel(t, t.DefaultEnabled) { LastAppliedSelection = t.DefaultEnabled };
            card.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TweakCardViewModel.IsPending))
                    OnPropertyChanged(nameof(PendingCount));
            };
            return card;
        }));

        _ = LoadRealStateAsync();
    }

    private async Task LoadRealStateAsync()
    {
        IsBusy = true;

        var tweaksSnapshot = Cards.Select(c => c.Tweak).ToList();
        var (latestEntries, appliedById) = await Task.Run(() =>
        {
            var entries = AppServices.Engine.LoadLatestEntriesByTweakId();
            var applied = tweaksSnapshot.ToDictionary(t => t.Id, SafeIsApplied);
            return (entries, applied);
        });

        foreach (var card in Cards)
        {
            var hasEntry = latestEntries.TryGetValue(card.Tweak.Id, out var entry);
            var isApplied = appliedById.TryGetValue(card.Tweak.Id, out var a) && a;
            var groundTruth = isApplied || (hasEntry && card.Tweak.DefaultEnabled);

            card.KnownBackupEntry = hasEntry ? entry : null;

            // If the user already toggled this card before the background check finished,
            // keep their choice - just correct what "applied" means so Pending stays accurate.
            var userAlreadyToggled = card.IsSelected != card.LastAppliedSelection;
            card.LastAppliedSelection = groundTruth;
            if (!userAlreadyToggled) card.IsSelected = groundTruth;
        }

        IsBusy = false;
    }

    private static bool SafeIsApplied(ITweak tweak)
    {
        try { return tweak.IsApplied(); }
        catch { return false; }
    }

    [RelayCommand]
    private async Task ApplyPendingAsync()
    {
        var pending = Cards.Where(c => c.IsPending).ToList();
        if (pending.Count == 0) return;

        IsBusy = true;
        StatusMessage = null;
        var applied = 0;
        var failed = 0;

        using var busy = BusyService.Instance.Begin($"Aplicando {pending.Count} cambio(s) en {Title}...");
        ActivityLog.Instance.Info($"{Title}: aplicando {pending.Count} cambio(s)...");

        await Task.Run(() =>
        {
            RestorePointGuard.EnsureBeforeChanges($"cambios en {Title}");


            foreach (var card in pending)
            {
                if (card.IsSelected && card.Risk != RiskLevel.Safe)
                {
                    var proceed = false;
                    Application.Current.Dispatcher.Invoke(() =>
                        proceed = ConfirmDialog.Ask(
                            $"Aplicar: {card.Name}",
                            $"{card.Description}\n\nEste cambio está marcado como {card.RiskLabel.ToUpperInvariant()}. " +
                            "Podrás revertirlo desde esta misma pantalla o desde la pestaña Backup.",
                            card.Risk));
                    if (!proceed)
                    {
                        card.IsSelected = false;
                        continue;
                    }
                }

                if (card.IsSelected)
                {
                    var (result, entry) = AppServices.Engine.ApplyOneWithEntry(card.Tweak, $"{Title} - {card.Name}");
                    if (result.Success)
                    {
                        applied++;
                        card.LastAppliedSelection = true;
                        if (entry is not null) card.KnownBackupEntry = entry;
                        ActivityLog.Instance.Ok($"Aplicado: {card.Name}");
                        if (card.Tweak.RequiresRestart)
                        {
                            PendingRestartService.Instance.MarkNeeded(card.Name);
                            ActivityLog.Instance.Warn($"«{card.Name}» necesita reiniciar para surtir efecto.");
                        }
                    }
                    else
                    {
                        failed++;
                        card.StatusMessage = result.Error;
                        card.IsSelected = card.LastAppliedSelection;
                        ActivityLog.Instance.Fail($"Falló «{card.Name}»: {result.Error}");
                    }
                }
                else
                {
                    if (card.KnownBackupEntry is BackupEntry knownEntry)
                    {
                        var result = AppServices.Engine.RevertOne(card.Tweak, knownEntry);
                        if (result.Success)
                        {
                            applied++;
                            card.LastAppliedSelection = false;
                            ActivityLog.Instance.Ok($"Revertido: {card.Name}");
                        }
                        else
                        {
                            failed++;
                            card.StatusMessage = result.Error;
                            card.IsSelected = card.LastAppliedSelection;
                            ActivityLog.Instance.Fail($"No se pudo revertir «{card.Name}»: {result.Error}");
                        }
                    }
                    else
                    {
                        card.StatusMessage = "No hay una copia previa para revertir este cambio.";
                        card.IsSelected = card.LastAppliedSelection;
                        ActivityLog.Instance.Warn($"«{card.Name}»: no hay copia previa para revertir.");
                    }
                }

                Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(PendingCount)));
            }
        });

        StatusMessage = failed == 0
            ? $"{applied} cambio(s) aplicados correctamente."
            : $"{applied} aplicados, {failed} con errores (revisa cada tarjeta).";
        if (failed == 0) ActivityLog.Instance.Ok($"{Title}: {applied} cambio(s) listos.");
        else ActivityLog.Instance.Warn($"{Title}: {applied} aplicados, {failed} con errores.");
        IsBusy = false;
        OnPropertyChanged(nameof(PendingCount));
    }
}
