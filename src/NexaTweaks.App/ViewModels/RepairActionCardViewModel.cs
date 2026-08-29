using CommunityToolkit.Mvvm.ComponentModel;
using NexaTweaks.Core;
using NexaTweaks.Core.Repair;

namespace NexaTweaks.App.ViewModels;

public partial class RepairActionCardViewModel : ObservableObject
{
    public RepairAction Action { get; }
    public string Name => Action.Name;
    public string Description => Action.Description;
    public RiskLevel Risk => Action.Risk;
    public string? EstimatedDuration => Action.EstimatedDuration;

    public string RiskLabel => Risk switch
    {
        RiskLevel.Safe => "Seguro",
        RiskLevel.Advanced => "Avanzado",
        RiskLevel.Risky => "Riesgo",
        _ => "",
    };

    [ObservableProperty] private bool isRunning;
    [ObservableProperty] private string? output;
    [ObservableProperty] private bool? lastSuccess;

    public RepairActionCardViewModel(RepairAction action) => Action = action;
}
