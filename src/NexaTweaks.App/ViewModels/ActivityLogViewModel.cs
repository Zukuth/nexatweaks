using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.Core.Diagnostics;
using NexaTweaks.Core.Settings;

namespace NexaTweaks.App.ViewModels;

/// <summary>Backs the bottom bar: the activity log plus the restore-point switch that guards
/// every batch of changes.</summary>
public partial class ActivityLogViewModel : ObservableObject
{
    public static ActivityLogViewModel Instance { get; } = new();

    public ObservableCollection<ActivityEntry> Entries { get; } = new();

    [ObservableProperty] private string statusText = "Listo.";
    [ObservableProperty] private bool createRestorePoint = AppPreferences.Instance.CreateRestorePoint;
    [ObservableProperty] private bool isLogVisible = true;

    private ActivityLogViewModel()
    {
        // The log starts filling at app startup, before this panel exists: adopt what's there.
        foreach (var entry in ActivityLog.Instance.Entries) Entries.Add(entry);
        if (Entries.Count > 0) statusText = Entries[^1].Message;
        ActivityLog.Instance.EntryAdded += OnEntryAdded;
        ActivityLog.Instance.Cleared += OnCleared;
    }

    partial void OnCreateRestorePointChanged(bool value)
    {
        AppPreferences.Instance.CreateRestorePoint = value;
        ActivityLog.Instance.Info(value
            ? "Punto de restauración activado: se creará uno antes de aplicar cambios."
            : "Punto de restauración desactivado.");
    }

    private void OnEntryAdded(ActivityEntry entry) => OnUi(() =>
    {
        Entries.Add(entry);
        while (Entries.Count > ActivityLog.DefaultCapacity) Entries.RemoveAt(0);
        StatusText = entry.Message;
    });

    private void OnCleared() => OnUi(() =>
    {
        Entries.Clear();
        StatusText = "Listo.";
    });

    private static void OnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess()) action();
        else dispatcher.BeginInvoke(action, DispatcherPriority.Background);
    }

    [RelayCommand]
    private void CopyLog()
    {
        try
        {
            Clipboard.SetText(ActivityLog.Instance.ExportText());
            ActivityLog.Instance.Ok("Registro copiado al portapapeles.");
        }
        catch (Exception ex)
        {
            AppLog.Error("Copiar registro", ex);
            ActivityLog.Instance.Fail($"No se pudo copiar el registro: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ClearLog() => ActivityLog.Instance.Clear();

    [RelayCommand]
    private void ToggleLog() => IsLogVisible = !IsLogVisible;
}
