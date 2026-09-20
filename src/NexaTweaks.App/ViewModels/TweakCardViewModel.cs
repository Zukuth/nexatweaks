using CommunityToolkit.Mvvm.ComponentModel;
using NexaTweaks.Core;
using NexaTweaks.Core.Backup;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.App.ViewModels;

public partial class TweakCardViewModel : ObservableObject
{
    public ITweak Tweak { get; }

    public string Name => Tweak.Name;
    public string Description => Tweak.Description;
    public RiskLevel Risk => Tweak.Risk;
    public bool IsReversible => Tweak.IsReversible;

    public string RiskLabel => Risk switch
    {
        RiskLevel.Safe => "Seguro",
        RiskLevel.Advanced => "Avanzado",
        RiskLevel.Risky => "Riesgo",
        _ => "",
    };

    [ObservableProperty]
    private bool isSelected;

    /// <summary>False when the tweak doesn't apply to this machine (a service Windows removed, or
    /// one of a program that isn't installed). Starts true so cards render immediately; the real
    /// value arrives with the background state check.</summary>
    [ObservableProperty]
    private bool isAvailable = true;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? statusMessage;

    /// <summary>Most recent backup entry known for this tweak (loaded from history or created this session).</summary>
    public BackupEntry? KnownBackupEntry { get; set; }

    private bool _lastAppliedSelection;

    /// <summary>
    /// The selection state as of the last successful apply/revert - used to compute "pending".
    /// A plain auto-property here wouldn't notify the UI when the engine updates it after a
    /// successful apply, leaving a stale "pendiente" label on screen even though nothing is
    /// pending anymore - so this raises IsPending/IsAppliedAndSynced explicitly on change.
    /// </summary>
    public bool LastAppliedSelection
    {
        get => _lastAppliedSelection;
        set
        {
            if (_lastAppliedSelection == value) return;
            _lastAppliedSelection = value;
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(IsAppliedAndSynced));
        }
    }

    public bool IsPending => IsSelected != LastAppliedSelection;

    /// <summary>True once a toggle is ON and the apply that turned it on has actually succeeded.</summary>
    public bool IsAppliedAndSynced => IsSelected && !IsPending;

    public TweakCardViewModel(ITweak tweak, bool initiallySelected)
    {
        Tweak = tweak;
        isSelected = initiallySelected;
        _lastAppliedSelection = initiallySelected;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsPending));
        OnPropertyChanged(nameof(IsAppliedAndSynced));
    }
}
