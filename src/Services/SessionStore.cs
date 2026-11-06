using System.IO;
using System.Text.Json;

namespace RiotSwitcherAccount;

public sealed class SessionStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public readonly string Root = AppPaths.DataRoot;
    public readonly string RiotData = AppPaths.RiotClientData;
    public string ConfigPath => Path.Combine(Root, "profiles.json");
    public string RecoveryPath => Path.Combine(Root, "last-recovery.bin");
    public SessionStore() => AppPaths.PrepareDataDirectory();

    public Preferences Load() => File.Exists(ConfigPath)
        ? JsonSerializer.Deserialize<Preferences>(File.ReadAllText(ConfigPath)) ?? new Preferences()
        : new Preferences();

    public void Save(Preferences state)
    {
        Directory.CreateDirectory(Root);
        var temp = ConfigPath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(state, Options));
        File.Move(temp, ConfigPath, true);
    }

    public string CurrentFile()
    {
        var first = Path.Combine(RiotData, "RiotGamesPrivateSettings.yaml");
        var second = Path.Combine(RiotData, "RiotClientPrivateSettings.yaml");
        if (File.Exists(first)) return first;
        if (File.Exists(second)) return second;
        return first;
    }

    public static bool LooksAuthenticated(byte[] raw)
    {
        // No se imprimen ni registran los tokens. Solo se comprueba una clave YAML.
        var text = System.Text.Encoding.UTF8.GetString(raw);
        return text.Contains("refresh_token:", StringComparison.OrdinalIgnoreCase)
            && !text.Contains("refresh_token: null", StringComparison.OrdinalIgnoreCase);
    }

    private string SnapshotFile(string id) => Path.Combine(Root, $"{id}.session.bin");

    public void SaveSnapshot(Profile profile, byte[] clear)
    {
        var encrypted = WindowsDpapi.Encrypt(clear);
        var target = SnapshotFile(profile.Id);
        var tmp = target + ".tmp";
        File.WriteAllBytes(tmp, encrypted);
        File.Move(tmp, target, true);
        profile.SavedUtc = DateTime.UtcNow;
        profile.Refresh();
    }

    public byte[] ReadSnapshot(Profile profile)
    {
        var file = SnapshotFile(profile.Id);
        if (!File.Exists(file)) throw new FileNotFoundException("No se encontró el respaldo cifrado de esta cuenta.");
        return WindowsDpapi.Decrypt(File.ReadAllBytes(file));
    }

    public void DeleteSnapshot(Profile profile)
    {
        var path = SnapshotFile(profile.Id);
        if (File.Exists(path)) File.Delete(path);
    }

    public void SaveRecovery(string fileName, byte[]? current)
    {
        if (current is null || current.Length == 0) return;
        // Se guarda el nombre del archivo en el prefijo, sin credenciales en texto plano.
        var data = new RecoveryPayload { Filename = fileName, Payload = Convert.ToBase64String(current) };
        File.WriteAllBytes(RecoveryPath, WindowsDpapi.Encrypt(JsonSerializer.SerializeToUtf8Bytes(data)));
    }

    public bool RestoreRecovery()
    {
        if (!File.Exists(RecoveryPath)) return false;
        var data = JsonSerializer.Deserialize<RecoveryPayload>(WindowsDpapi.Decrypt(File.ReadAllBytes(RecoveryPath)));
        if (data is null || !ValidSessionFileName(data.Filename)) return false;
        WriteSession(data.Filename, Convert.FromBase64String(data.Payload));
        return true;
    }

    public static bool ValidSessionFileName(string name) => name is "RiotGamesPrivateSettings.yaml" or "RiotClientPrivateSettings.yaml";

    public void WriteSession(string fileName, byte[] contents)
    {
        if (!ValidSessionFileName(fileName)) throw new InvalidOperationException("Nombre de sesión de Riot inesperado.");
        Directory.CreateDirectory(RiotData);
        var target = Path.Combine(RiotData, fileName);
        var tmp = target + ".riotswitcher.tmp";
        try { File.WriteAllBytes(tmp, contents); File.Move(tmp, target, true); }
        finally { if (File.Exists(tmp)) File.Delete(tmp); }
    }

    private sealed class RecoveryPayload
    {
        public string Filename { get; set; } = "";
        public string Payload { get; set; } = "";
    }
}

