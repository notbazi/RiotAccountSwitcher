using System.IO;

namespace RiotSwitcherAccount;

/// <summary>All application-owned files live in the current Windows user's local profile.</summary>
internal static class AppPaths
{
    private static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static readonly string DataRoot = Path.Combine(LocalAppData, "RiotSwitcherAccount");
    private static readonly string OldDataRoot = Path.Combine(LocalAppData, "OxocarbonRiot");
    public static readonly string RiotClientData = Path.Combine(LocalAppData, "Riot Games", "Riot Client", "Data");

    /// <summary>
    /// Migrates the previous local data using a staging directory and an atomic directory rename.
    /// Never deletes or modifies the old encrypted backups. DPAPI remains valid for the same user.
    /// </summary>
    public static void PrepareDataDirectory()
    {
        if (Directory.Exists(DataRoot))
        {
            // A previous failed startup may have created only the diagnostics folder.
            // Preserve existing new-format data. Import old profiles only if none exist yet.
            if (!File.Exists(Path.Combine(DataRoot, "profiles.json")) &&
                File.Exists(Path.Combine(OldDataRoot, "profiles.json")))
                CopyCompatibleFiles(OldDataRoot, DataRoot);
            return;
        }
        if (!Directory.Exists(OldDataRoot))
        {
            Directory.CreateDirectory(DataRoot);
            return;
        }

        var stage = DataRoot + ".migration-" + Guid.NewGuid().ToString("N");
        try
        {
            Directory.CreateDirectory(stage);
            CopyCompatibleFiles(OldDataRoot, stage);
            Directory.Move(stage, DataRoot);
        }
        catch
        {
            try { if (Directory.Exists(stage)) Directory.Delete(stage, recursive: true); }
            catch { /* Preserve the original failure; old user data remains untouched. */ }
            throw;
        }
    }

    private static void CopyCompatibleFiles(string origin, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(origin, "*", SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileName(file);
            if (name is "profiles.json" or "last-recovery.bin" || IsSessionName(name))
            {
                var target = Path.Combine(destination, name);
                if (!File.Exists(target)) File.Copy(file, target);
            }
        }
        var oldIcons = Path.Combine(origin, "icons");
        if (!Directory.Exists(oldIcons)) return;
        var newIcons = Path.Combine(destination, "icons");
        Directory.CreateDirectory(newIcons);
        foreach (var file in Directory.EnumerateFiles(oldIcons, "*.png", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!Guid.TryParseExact(name, "N", out _)) continue;
            var target = Path.Combine(newIcons, Path.GetFileName(file));
            if (!File.Exists(target)) File.Copy(file, target);
        }
    }

    private static bool IsSessionName(string name) =>
        name.EndsWith(".session.bin", StringComparison.OrdinalIgnoreCase)
        && Guid.TryParseExact(name.Substring(0, name.Length - ".session.bin".Length), "N", out _);
}
