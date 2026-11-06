using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace RiotSwitcherAccount;

/// <summary>Local-only crash diagnostic. Does not read or record Riot session files.</summary>
internal static class CrashReporter
{
    public static void Show(Exception exception, string phase)
    {
        string type = exception.GetType().FullName ?? exception.GetType().Name;
        string location = "";
        try
        {
            string folder = Path.Combine(AppPaths.DataRoot, "diagnostics");
            Directory.CreateDirectory(folder);
            location = Path.Combine(folder, "last-error.txt");
            // Keep diagnostics local. Log the exception chain (not the session files) so
            // WPF XAML inner exceptions are visible when troubleshooting startup errors.
            // Review this text for local file paths before choosing to share it.
            var details = new StringBuilder()
                .AppendLine($"UTC: {DateTime.UtcNow:O}")
                .AppendLine($"Phase: {phase}");
            Exception? current = exception;
            int depth = 0;
            while (current is not null && depth < 6)
            {
                details.AppendLine($"Exception[{depth}]: {current.GetType().FullName}")
                    .AppendLine($"HRESULT[{depth}]: 0x{current.HResult:X8}")
                    .AppendLine($"Message[{depth}]: {current.Message}");
                if (current is XamlParseException xaml)
                    details.AppendLine($"XamlLine: {xaml.LineNumber}; XamlPosition: {xaml.LinePosition}");
                depth++;
                current = current.InnerException;
            }
            details.AppendLine("Stack:").AppendLine(exception.StackTrace ?? "(not available)");
            File.WriteAllText(location, details.ToString(), Encoding.UTF8);
        }
        catch { /* A failed diagnostic must not obscure the original failure. */ }

        var pathText = string.IsNullOrEmpty(location) ? "Could not write diagnostic log." : $"Log: {location}";
        var rootCause = exception;
        while (rootCause.InnerException is not null) rootCause = rootCause.InnerException;
        MessageBox.Show($"Riot Account Switcher could not start correctly.\n\n{type}\n{exception.Message}\n\nRoot cause: {rootCause.GetType().Name}: {rootCause.Message}\n\n{pathText}",
            "Riot Account Switcher - Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
