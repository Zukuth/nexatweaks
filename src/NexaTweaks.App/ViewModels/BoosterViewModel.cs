using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.Core.Booster;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App.ViewModels;

public partial class BoosterViewModel : ObservableObject
{
    private readonly DispatcherTimer _autoDetectTimer;
    private bool _autoDetectTickRunning;

    public ObservableCollection<GameProfile> Profiles { get; } = new();

    [ObservableProperty] private GameProfile? selectedProfile;
    [ObservableProperty] private string customProcessName = "";
    [ObservableProperty] private bool isBoosting;
    [ObservableProperty] private string? boostedProcessName;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private string newGameName = "";
    [ObservableProperty] private string newGameProcess = "";
    [ObservableProperty] private bool autoDetectEnabled;
    [ObservableProperty] private bool timerResolutionEnabled = true;

    public BoosterViewModel()
    {
        foreach (var profile in AppServices.GameProfiles.Load())
            Profiles.Add(profile);
        SelectedProfile = Profiles.FirstOrDefault();

        _autoDetectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _autoDetectTimer.Tick += async (_, _) => await OnAutoDetectTickAsync();
    }

    partial void OnAutoDetectEnabledChanged(bool value)
    {
        if (value) _autoDetectTimer.Start();
        else _autoDetectTimer.Stop();
    }

    private async Task OnAutoDetectTickAsync()
    {
        // Enumerating every process on the system isn't free - skip this tick if the previous
        // one is still running instead of piling up overlapping background scans.
        if (_autoDetectTickRunning) return;
        _autoDetectTickRunning = true;

        try
        {
            var profilesSnapshot = Profiles.ToList();
            var running = await Task.Run(() => ProcessBoosterService.FindRunningProfile(profilesSnapshot));

            if (running is null)
            {
                if (AppServices.Booster.IsBoosting && !IsUserManagedBoost)
                {
                    await Task.Run(() => AppServices.Booster.Stop());
                    IsBoosting = false;
                    BoostedProcessName = null;
                    StatusMessage = "Juego cerrado: boost detenido automáticamente.";
                }
                return;
            }

            if (!AppServices.Booster.IsBoosting)
            {
                var ok = await Task.Run(() => AppServices.Booster.Start(running.ProcessName, TimerResolutionEnabled));
                if (ok)
                {
                    IsBoosting = true;
                    BoostedProcessName = running.ProcessName;
                    StatusMessage = $"\"{running.Name}\" detectado: boost aplicado automáticamente.";
                }
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Booster auto-detect tick", ex);
        }
        finally
        {
            _autoDetectTickRunning = false;
        }
    }

    /// <summary>True while the user started the boost by hand (as opposed to auto-detect) so auto-detect doesn't stop it under them.</summary>
    private bool IsUserManagedBoost { get; set; }

    [RelayCommand]
    private async Task ToggleBoostAsync()
    {
        if (AppServices.Booster.IsBoosting)
        {
            await Task.Run(() => AppServices.Booster.Stop());
            IsBoosting = false;
            BoostedProcessName = null;
            IsUserManagedBoost = false;
            StatusMessage = "Boost detenido, prioridades restauradas.";
            return;
        }

        var processName = string.IsNullOrWhiteSpace(CustomProcessName)
            ? SelectedProfile?.ProcessName
            : CustomProcessName.Trim();

        if (string.IsNullOrWhiteSpace(processName))
        {
            StatusMessage = "Selecciona un juego o escribe el nombre del proceso.";
            return;
        }

        var found = await Task.Run(() =>
        {
            var running = Process.GetProcessesByName(processName);
            var any = running.Length > 0;
            foreach (var p in running) p.Dispose();
            return any;
        });

        if (!found)
        {
            StatusMessage = $"No se encontró ningún proceso llamado \"{processName}\" en ejecución. Abre el juego primero.";
            return;
        }

        var ok = await Task.Run(() => AppServices.Booster.Start(processName, TimerResolutionEnabled));
        IsBoosting = ok;
        BoostedProcessName = ok ? processName : null;
        IsUserManagedBoost = ok;
        StatusMessage = ok
            ? $"Boost activo sobre \"{processName}\": prioridad alta + apps en segundo plano reducidas" +
              (TimerResolutionEnabled ? " + resolución de temporizador alta." : ".")
            : "No se pudo aplicar el boost a ese proceso.";
    }

    [RelayCommand]
    private void AddProfile()
    {
        if (string.IsNullOrWhiteSpace(NewGameName) || string.IsNullOrWhiteSpace(NewGameProcess)) return;

        var profile = new GameProfile(NewGameName.Trim(), NewGameProcess.Trim());
        Profiles.Add(profile);
        AppServices.GameProfiles.Save(Profiles);
        SelectedProfile = profile;
        NewGameName = "";
        NewGameProcess = "";
    }

    [RelayCommand]
    private void RemoveProfile(GameProfile profile)
    {
        Profiles.Remove(profile);
        AppServices.GameProfiles.Save(Profiles);
    }
}
