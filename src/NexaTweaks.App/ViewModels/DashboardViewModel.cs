using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NexaTweaks.App.Services;
using NexaTweaks.Core;
using NexaTweaks.Core.Catalog;
using NexaTweaks.Core.Diagnostics;
using NexaTweaks.Core.Tweaks;

namespace NexaTweaks.App.ViewModels;

public sealed record MissingOptimizationRowViewModel(string Name, string CategoryLabel);

public partial class DashboardViewModel : ObservableObject, IDisposable
{
    private const int HistoryLength = 40;
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private double cpuPercent;
    [ObservableProperty] private double ramPercent;
    [ObservableProperty] private double ramUsedGb;
    [ObservableProperty] private double ramTotalGb;
    [ObservableProperty] private double gpuPercent;
    [ObservableProperty] private double? cpuTempCelsius;
    [ObservableProperty] private double diskFreeGb;
    [ObservableProperty] private double diskTotalGb;
    [ObservableProperty] private long? pingMs;
    [ObservableProperty] private double netDownKbps;
    [ObservableProperty] private double netUpKbps;
    [ObservableProperty] private double cpuMhz;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private string? deltaMessage;
    [ObservableProperty] private string? bottleneckMessage;

    [ObservableProperty] private bool isAnalyzing;
    [ObservableProperty] private bool hasAnalyzed;
    [ObservableProperty] private double optimizationScore;
    [ObservableProperty] private int appliedRecommendedCount;
    [ObservableProperty] private int totalRecommendedCount;

    public ObservableCollection<MissingOptimizationRowViewModel> MissingOptimizations { get; } = new();

    [ObservableProperty] private string cpuName = "";
    [ObservableProperty] private string gpuName = "";
    [ObservableProperty] private string osName = "";
    [ObservableProperty] private string motherboard = "";
    public ObservableCollection<RamModuleInfo> RamModules { get; } = new();
    public ObservableCollection<SystemBadge> WindowsBadges { get; } = new();
    public ObservableCollection<SystemBadge> BoardBadges { get; } = new();

    /// <summary>Used space of the system drive, for the storage card.</summary>
    public double DiskUsedGb => Math.Max(0, DiskTotalGb - DiskFreeGb);
    public double DiskUsedPercent => DiskTotalGb > 0 ? DiskUsedGb / DiskTotalGb * 100 : 0;

    public List<double> CpuHistory { get; } = new();
    public List<double> RamHistory { get; } = new();
    public List<double> GpuHistory { get; } = new();

    public event Action? HistoryUpdated;

    public DashboardViewModel()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
        _timer.Tick += async (_, _) =>
        {
            try { await SampleAsync(); }
            catch (Exception ex) { AppLog.Error("Dashboard sample tick", ex); }
        };
        _timer.Start();
        _ = SampleAsync();
        _ = LoadOverviewAsync();
    }

    /// <summary>The machine description comes from WMI, which is slow - load it once, off the UI thread.</summary>
    private async Task LoadOverviewAsync()
    {
        try
        {
            var overview = await Task.Run(SystemOverviewService.Get);
            CpuName = overview.CpuName;
            GpuName = overview.GpuName;
            OsName = overview.OsName;
            Motherboard = overview.Motherboard;
            foreach (var module in overview.RamModules) RamModules.Add(module);
            foreach (var badge in overview.WindowsBadges) WindowsBadges.Add(badge);
            foreach (var badge in overview.BoardBadges) BoardBadges.Add(badge);
        }
        catch (Exception ex)
        {
            AppLog.Error("Dashboard overview", ex);
        }
    }

    public void Pause() => _timer.Stop();
    public void Resume() => _timer.Start();

    private async Task SampleAsync()
    {
        var stats = await Task.Run(() => AppServices.Stats.Sample());
        CpuPercent = stats.CpuPercent;
        RamPercent = stats.RamPercent;
        RamUsedGb = stats.RamUsedGb;
        RamTotalGb = stats.RamTotalGb;
        GpuPercent = stats.GpuPercent;
        CpuTempCelsius = stats.CpuTempCelsius;
        DiskFreeGb = stats.DiskFreeGb;
        DiskTotalGb = stats.DiskTotalGb;
        NetDownKbps = stats.NetDownKbps;
        NetUpKbps = stats.NetUpKbps;
        CpuMhz = stats.CpuMhz;
        OnPropertyChanged(nameof(DiskUsedGb));
        OnPropertyChanged(nameof(DiskUsedPercent));

        Push(CpuHistory, stats.CpuPercent);
        Push(RamHistory, stats.RamPercent);
        Push(GpuHistory, stats.GpuPercent);
        HistoryUpdated?.Invoke();
        UpdateBottleneckMessage();

        PingMs = await AppServices.Ping.PingMsAsync();
    }

    private void UpdateBottleneckMessage()
    {
        const int window = 8;
        if (CpuHistory.Count < window) { BottleneckMessage = null; return; }

        var avgCpu = CpuHistory.TakeLast(window).Average();
        var avgGpu = GpuHistory.TakeLast(window).Average();

        if (RamPercent >= 90)
            BottleneckMessage = "RAM casi llena (>90%): esto puede causar tirones. Prueba 'Vaciar RAM en espera' en Limpieza.";
        else if (avgGpu >= 5 && avgCpu - avgGpu > 25 && avgCpu > 70)
            BottleneckMessage = "Posible cuello de botella de CPU: tu procesador está mucho más cargado que la GPU.";
        else if (avgGpu - avgCpu > 25 && avgGpu > 85)
            BottleneckMessage = "Tu GPU está al límite: es el componente que más carga tiene ahora mismo.";
        else
            BottleneckMessage = null;
    }

    private static void Push(List<double> history, double value)
    {
        history.Add(value);
        if (history.Count > HistoryLength) history.RemoveAt(0);
    }

    private static List<ITweak> GetRecommendedTweaks() =>
        TweakCatalog.Windows
            .Concat(TweakCatalog.Network)
            .Concat(TweakCatalog.Input)
            .Concat(TweakCatalog.Gpu)
            .Concat(TweakCatalog.Cleanup)
            .Where(t => t.Risk == RiskLevel.Safe && t.DefaultEnabled)
            .ToList();

    private static string CategoryLabel(TweakCategory category) => category switch
    {
        TweakCategory.Windows => "Windows",
        TweakCategory.Network => "Red",
        TweakCategory.Input => "Input",
        TweakCategory.Gpu => "GPU",
        TweakCategory.Cleanup => "Limpieza",
        TweakCategory.Booster => "Booster",
        TweakCategory.Advanced => "Avanzado",
        _ => category.ToString(),
    };

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        IsAnalyzing = true;
        using var busy = BusyService.Instance.Begin("Analizando qué optimizaciones te faltan...");
        ActivityLog.Instance.Info("Dashboard: analizando optimizaciones recomendadas...");

        var (score, applied, total, missing) = await Task.Run(() =>
        {
            var recommended = GetRecommendedTweaks();
            var appliedList = new List<ITweak>();
            var missingList = new List<ITweak>();

            foreach (var tweak in recommended)
                (SafeIsApplied(tweak) ? appliedList : missingList).Add(tweak);

            var pct = recommended.Count == 0 ? 100.0 : appliedList.Count * 100.0 / recommended.Count;
            return (pct, appliedList.Count, recommended.Count, missingList);
        });

        OptimizationScore = score;
        AppliedRecommendedCount = applied;
        TotalRecommendedCount = total;

        MissingOptimizations.Clear();
        foreach (var tweak in missing)
            MissingOptimizations.Add(new MissingOptimizationRowViewModel(tweak.Name, CategoryLabel(tweak.Category)));

        HasAnalyzed = true;
        IsAnalyzing = false;
        ActivityLog.Instance.Info(
            $"Dashboard: {applied} de {total} optimizaciones aplicadas ({score:F0}%). Faltan {MissingOptimizations.Count}.");
    }

    [RelayCommand]
    private async Task ApplyRecommendedAsync()
    {
        IsBusy = true;
        StatusMessage = "Midiendo estado inicial...";

        var beforeRam = RamUsedGb;
        var beforePing = PingMs;

        using var busy = BusyService.Instance.Begin("Aplicando tweaks recomendados...");

        // Filtering "already applied" touches the registry/services/WMI per tweak, which can
        // stall the UI thread if done here directly - so the whole selection + apply pass runs
        // in the background, same as the actual apply step.
        var (_, results) = await Task.Run(() =>
        {
            var pending = GetRecommendedTweaks().Where(t => !SafeIsApplied(t)).ToList();
            ActivityLog.Instance.Info($"Dashboard: aplicando {pending.Count} tweak(s) recomendado(s)...");
            RestorePointGuard.EnsureBeforeChanges("tweaks recomendados del Dashboard");
            return AppServices.Engine.ApplyMany(pending, "Aplicar recomendado (Dashboard)");
        });

        foreach (var r in results.Where(r => r.Success)) ActivityLog.Instance.Ok($"Aplicado: {r.Tweak.Name}");
        foreach (var r in results.Where(r => !r.Success)) ActivityLog.Instance.Fail($"Falló «{r.Tweak.Name}»: {r.Error}");

        foreach (var r in results.Where(r => r.Success && r.Tweak.RequiresRestart))
            PendingRestartService.Instance.MarkNeeded(r.Tweak.Name);

        await SampleAsync();
        await AnalyzeAsync();

        var afterRam = RamUsedGb;
        var afterPing = PingMs;
        var success = results.Count(r => r.Success);
        var failed = results.Count(r => !r.Success);

        var ramDelta = beforeRam - afterRam;
        var pingText = beforePing is not null && afterPing is not null
            ? $" · Ping {beforePing}ms → {afterPing}ms"
            : "";

        StatusMessage = $"{success} tweak(s) aplicados" + (failed > 0 ? $", {failed} con errores" : "") + ".";
        DeltaMessage = $"RAM en uso: {beforeRam:F1} GB → {afterRam:F1} GB ({(ramDelta >= 0 ? "-" : "+")}{Math.Abs(ramDelta):F1} GB){pingText}";
        IsBusy = false;
    }

    private static bool SafeIsApplied(ITweak tweak)
    {
        try { return tweak.IsApplied(); } catch { return false; }
    }

    public void Dispose() => _timer.Stop();
}
