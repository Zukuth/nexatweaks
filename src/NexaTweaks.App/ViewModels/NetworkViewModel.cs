using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Monitoring;

namespace NexaTweaks.App.ViewModels;

public sealed record DnsResultRowViewModel(string Provider, string Ip, string LatencyText, bool IsFastest);

public partial class NetworkViewModel : ObservableObject
{
    public TweakCategoryViewModel Tweaks { get; }
    public ObservableCollection<DnsResultRowViewModel> DnsResults { get; } = new();

    [ObservableProperty] private bool isBenchmarking;
    [ObservableProperty] private string? benchmarkStatus;

    public NetworkViewModel()
    {
        Tweaks = new TweakCategoryViewModel(
            "Red", "Latencia, ping y ancho de banda: los tweaks de red clásicos para competitivo.", TweakCatalog.Network);
    }

    [RelayCommand]
    private async Task RunDnsBenchmarkAsync()
    {
        IsBenchmarking = true;
        BenchmarkStatus = null;
        DnsResults.Clear();

        using var busy = BusyService.Instance.Begin("Midiendo latencia de los servidores DNS...");
        var results = await DnsBenchmarkService.RunAsync();
        var fastest = results.Where(r => r.RoundtripMs is not null).MinBy(r => r.RoundtripMs);

        foreach (var r in results)
        {
            var latency = r.RoundtripMs is long ms ? $"{ms} ms" : "sin respuesta";
            DnsResults.Add(new DnsResultRowViewModel(r.Provider, r.Ip, latency, r == fastest));
        }

        BenchmarkStatus = fastest is not null
            ? $"Más rápido: {fastest.Provider} ({fastest.RoundtripMs} ms). Puedes activar el tweak \"DNS rápido\" si es Cloudflare."
            : "No se pudo medir ningún proveedor (revisa tu conexión).";
        IsBenchmarking = false;
    }
}
