using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using NexaTweaks.App.Services;
using NexaTweaks.Core;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App.Views;

public partial class RestartBanner : UserControl
{
    public RestartBanner()
    {
        InitializeComponent();
        DataContext = PendingRestartService.Instance;
    }

    private void RestartButton_Click(object sender, RoutedEventArgs e)
    {
        var proceed = ConfirmDialog.Ask(
            "Reiniciar Windows",
            "Esto cerrará todos tus programas abiertos y reiniciará el equipo en unos segundos. Guarda tu trabajo antes de continuar.",
            RiskLevel.Risky);

        if (!proceed) return;

        try
        {
            Process.Start(new ProcessStartInfo("shutdown.exe", "/r /t 15 /c \"Nexa Tweaks: reiniciando para aplicar cambios pendientes\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
            });
        }
        catch (Exception ex)
        {
            AppLog.Error("Restart banner", ex);
            MessageBox.Show($"No se pudo iniciar el reinicio: {ex.Message}", "Nexa Tweaks",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DismissButton_Click(object sender, RoutedEventArgs e) => PendingRestartService.Instance.Dismiss();
}
