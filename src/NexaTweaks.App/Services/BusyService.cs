using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NexaTweaks.App.Services;

/// <summary>
/// App-wide "something is happening" signal. Any long-running operation calls Begin(message)
/// and disposes the returned scope when done; the main window shows a full-screen overlay for
/// as long as at least one scope is open, so the user always has clear feedback that the app is
/// working instead of wondering if a click did anything.
/// </summary>
public sealed partial class BusyService : ObservableObject
{
    public static BusyService Instance { get; } = new();

    private int _activeCount;

    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? message;

    private BusyService() { }

    public IDisposable Begin(string message)
    {
        RunOnUi(() =>
        {
            _activeCount++;
            Message = message;
            IsBusy = true;
        });
        return new Scope(this);
    }

    private void End()
    {
        RunOnUi(() =>
        {
            _activeCount = Math.Max(0, _activeCount - 1);
            if (_activeCount == 0)
            {
                IsBusy = false;
                Message = null;
            }
        });
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess()) action();
        else dispatcher.Invoke(action);
    }

    private sealed class Scope : IDisposable
    {
        private readonly BusyService _owner;
        private bool _disposed;
        public Scope(BusyService owner) => _owner = owner;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _owner.End();
        }
    }
}
