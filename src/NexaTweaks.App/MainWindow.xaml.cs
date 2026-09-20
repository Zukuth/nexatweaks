using System.Windows;
using System.Windows.Controls;
using NexaTweaks.App.ViewModels;
using NexaTweaks.App.Views;
using NexaTweaks.Core;
using NexaTweaks.Core.Catalog;

namespace NexaTweaks.App;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, UserControl> _cache = new();

    public MainWindow()
    {
        InitializeComponent();
        ShowPage("Dashboard");
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string key }) ShowPage(key);
    }

    private void ShowPage(string key)
    {
        if (!_cache.TryGetValue(key, out var view))
        {
            view = CreateView(key);
            _cache[key] = view;
        }
        ContentHost.Content = view;
    }

    private static UserControl CreateView(string key) => key switch
    {
        "Dashboard" => new DashboardView(new DashboardViewModel()),
        "Windows" => new TweakCategoryView(new TweakCategoryViewModel(
            "Windows", "Servicios, telemetría, efectos visuales y plan de energía.", TweakCatalog.Windows)),
        "Presets" => new PresetsView(new PresetsViewModel()),
        "Debloat" => new TweakCategoryView(new TweakCategoryViewModel(
            "Debloatware", "Apps preinstaladas de la Tienda. Desinstalar es de ida: para recuperarlas hay que bajarlas de Microsoft Store.",
            TweakCatalog.Debloat)),
        "Privacy" => new TweakCategoryView(new TweakCategoryViewModel(
            "Privacidad", "Telemetría, Copilot, Recall, widgets y permisos de las apps de la Tienda.", TweakCatalog.Privacy)),
        "Services" => new TweakCategoryView(new TweakCategoryViewModel(
            "Servicios", "Servicios de Windows que un PC doméstico no necesita. Windows Update y Defender nunca se tocan.", TweakCatalog.Services)),
        "Interface" => new TweakCategoryView(new TweakCategoryViewModel(
            "Interfaz", "Barra de tareas y Explorador a tu gusto. Algunos cambios piden reiniciar el Explorador.", TweakCatalog.Interface)),
        "Network" => new NetworkView(new NetworkViewModel()),
        "Input" => new TweakCategoryView(new TweakCategoryViewModel(
            "Input", "Mouse y teclado sin aceleración, respuesta 1:1.", TweakCatalog.Input)),
        "Gpu" => new TweakCategoryView(new TweakCategoryViewModel(
            "GPU", "Programación de GPU, Game Bar y prioridad para el juego en primer plano.", TweakCatalog.Gpu)),
        "Cleanup" => new CleanupView(new CleanupViewModel()),
        "Booster" => new BoosterView(new BoosterViewModel()),
        "Apps" => new AppsView(new AppsViewModel()),
        "Autorun" => new AutorunView(new AutorunViewModel()),
        "Repair" => new RepairView(new RepairViewModel()),
        "Stability" => new StabilityView(new StabilityViewModel()),
        "Backup" => new BackupView(new BackupViewModel()),
        "Advanced" => new TweakCategoryView(new TweakCategoryViewModel(
            "Avanzado", "Tweaks de mayor riesgo (Defender, Windows Update, UAC, Xbox). Desactivados por defecto y con confirmación extra.",
            TweakCatalog.Advanced)),
        "Settings" => new SettingsView(new SettingsViewModel()),
        _ => throw new ArgumentOutOfRangeException(nameof(key)),
    };
}
