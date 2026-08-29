using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.App.Views;
using NexaTweaks.Core;
using NexaTweaks.Core.Repair;

namespace NexaTweaks.App.ViewModels;

public partial class RepairViewModel : ObservableObject
{
    public ObservableCollection<RepairActionCardViewModel> Actions { get; }

    public RepairViewModel()
    {
        Actions = new ObservableCollection<RepairActionCardViewModel>(
            RepairCatalog.Actions.Select(a => new RepairActionCardViewModel(a)));
    }

    [RelayCommand]
    private async Task RunAsync(RepairActionCardViewModel card)
    {
        if (card.Risk != RiskLevel.Safe)
        {
            var proceed = ConfirmDialog.Ask(
                $"Ejecutar: {card.Name}",
                $"{card.Description}\n\nEsta acción está marcada como {card.RiskLabel.ToUpperInvariant()}.",
                card.Risk);
            if (!proceed) return;
        }

        card.IsRunning = true;
        card.Output = null;
        card.LastSuccess = null;

        RepairActionResult result;
        using (BusyService.Instance.Begin($"Ejecutando {card.Name}...\n{card.EstimatedDuration}"))
        {
            result = await Task.Run(() => card.Action.Run());
        }

        card.Output = result.Output;
        card.LastSuccess = result.Success;
        card.IsRunning = false;

        if (result.Success && card.Action.RequiresRestart)
            PendingRestartService.Instance.MarkNeeded(card.Name);
    }
}
