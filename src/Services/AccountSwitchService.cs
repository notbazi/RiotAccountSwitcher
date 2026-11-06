using System.Diagnostics;
using System.IO;

namespace RiotSwitcherAccount;

/// <summary>
/// Switches an existing Riot Client session. Both the compact account chooser
/// and the account-management window use this same implementation.
/// It never receives or saves account passwords.
/// </summary>
internal static class AccountSwitchService
{
    public static void Switch(SessionStore store, Preferences preferences, Profile chosen)
    {
        string currentPath = store.CurrentFile();
        byte[]? oldBytes = File.Exists(currentPath) ? File.ReadAllBytes(currentPath) : null;
        string previousFile = Path.GetFileName(currentPath);
        string? oldActiveId = preferences.ActiveId;
        byte[] newSession = store.ReadSnapshot(chosen);
        if (!SessionStore.LooksAuthenticated(newSession))
            throw new InvalidOperationException(UiStrings.T("SnapshotInvalid"));

        if (oldBytes is not null)
        {
            store.SaveRecovery(previousFile, oldBytes);
            if (!string.IsNullOrEmpty(oldActiveId) && oldActiveId != chosen.Id && SessionStore.LooksAuthenticated(oldBytes))
            {
                var active = preferences.Profiles.FirstOrDefault(p => p.Id == oldActiveId);
                if (active is not null) store.SaveSnapshot(active, oldBytes);
            }
        }

        var targetPath = Path.Combine(store.RiotData, chosen.Filename);
        byte[]? beforeTarget = File.Exists(targetPath) ? File.ReadAllBytes(targetPath) : null;
        try
        {
            store.WriteSession(chosen.Filename, newSession);
            LaunchClient(preferences.RiotExe);
        }
        catch
        {
            if (beforeTarget is not null) store.WriteSession(chosen.Filename, beforeTarget);
            else if (File.Exists(targetPath)) File.Delete(targetPath);
            throw;
        }

        preferences.ActiveId = chosen.Id;
        chosen.LastUsedUtc = DateTime.UtcNow;
        store.Save(preferences);
    }

    public static void LaunchClient(string? executable)
    {
        string path = executable?.Trim().Trim('"') ?? string.Empty;
        if (!File.Exists(path)) throw new FileNotFoundException(UiStrings.T("LauncherNotFound"), path);
        if (!string.Equals(Path.GetFileName(path), "RiotClientServices.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(UiStrings.T("LauncherWrong"));
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = path,
            WorkingDirectory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory,
            UseShellExecute = true
        });
        if (process is null) throw new InvalidOperationException(UiStrings.T("LauncherStartError"));
    }
}
