using System.Windows;
using System.Windows.Threading;
using NexaTweaks.Core.Diagnostics;

namespace NexaTweaks.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // A single unhandled exception on the UI thread, a background Task, or a stray thread
        // used to kill the whole app silently. Catch all three, log them, and keep the app alive
        // whenever it's safe to do so.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        AppLog.Error("UI thread", e.Exception);
        MessageBox.Show(
            $"Ocurrió un error inesperado y NexaTweaks lo interceptó antes de que cerrara la app.\n\n{e.Exception.Message}\n\nDetalles guardados en el registro de errores (Ajustes > Carpeta de copias de seguridad).",
            "NexaTweaks - Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex) AppLog.Error("AppDomain (fatal)", ex);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        AppLog.Error("Background task", e.Exception);
        e.SetObserved();
    }
}
