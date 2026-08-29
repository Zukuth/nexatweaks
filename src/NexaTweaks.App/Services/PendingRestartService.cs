using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NexaTweaks.App.Services;

/// <summary>
/// Tracks whether any applied tweak/repair action still needs a Windows restart to fully take
/// effect, so the app can show a persistent "Reiniciar ahora" banner instead of leaving the
/// user to guess why a change doesn't seem to have done anything yet.
/// </summary>
public sealed partial class PendingRestartService : ObservableObject
{
    public static PendingRestartService Instance { get; } = new();

    [ObservableProperty] private bool isRestartNeeded;

    public ObservableCollection<string> Reasons { get; } = new();

    public string ReasonsText => string.Join(", ", Reasons);

    private PendingRestartService() { }

    public void MarkNeeded(string reason)
    {
        RunOnUi(() =>
        {
            if (!Reasons.Contains(reason)) Reasons.Add(reason);
            OnPropertyChanged(nameof(ReasonsText));
            IsRestartNeeded = true;
        });
    }

    public void Dismiss()
    {
        RunOnUi(() =>
        {
            IsRestartNeeded = false;
            Reasons.Clear();
            OnPropertyChanged(nameof(ReasonsText));
        });
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action);
    }
}
