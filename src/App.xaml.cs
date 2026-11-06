using System.Windows;
using System.Windows.Threading;

namespace RiotSwitcherAccount;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        try
        {
            // Initialize text resources before WPF parses windows with DynamicResource bindings.
            UiStrings.ApplyLanguage("en");
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            CrashReporter.Show(ex, "Startup");
            Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        CrashReporter.Show(e.Exception, "Unhandled UI error");
        e.Handled = true;
        Shutdown(1);
    }
}
