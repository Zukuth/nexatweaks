using CommunityToolkit.Mvvm.ComponentModel;
using NexaTweaks.Core.Stability;

namespace NexaTweaks.App.ViewModels;

public partial class StabilityFindingViewModel : ObservableObject
{
    public StabilityFinding Finding { get; }
    public string Title => Finding.Title;
    public StabilitySeverity Severity => Finding.Severity;
    public string Recommendation => Finding.Recommendation;
    public string EvidenceText => string.Join("\n", Finding.Evidence.Select(e => $"• {e}"));
    public string? FixName => Finding.Fix?.Name;

    public string SeverityLabel => Severity switch
    {
        StabilitySeverity.High => "Grave",
        StabilitySeverity.Medium => "Media",
        StabilitySeverity.Low => "Leve",
        _ => "Info",
    };

    public bool CanFix => Finding.Fix is not null && !IsFixed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanFix))]
    private bool isFixed;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;

    public StabilityFindingViewModel(StabilityFinding finding) => Finding = finding;
}
