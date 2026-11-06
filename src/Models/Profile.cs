using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace RiotSwitcherAccount;

public sealed class Profile : INotifyPropertyChanged
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Alias { get; set; } = "";
    public string Filename { get; set; } = "RiotGamesPrivateSettings.yaml";
    public DateTime SavedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedUtc { get; set; }
    // 'riot' is the safe neutral icon for profiles created by older versions.
    public string IconKind { get; set; } = "riot";
    // These properties contain public profile identifiers only, never credentials.
    public string GameKind { get; set; } = "riot";
    public string ProfileSource { get; set; } = "";
    public string ProfileRegion { get; set; } = "las";
    [JsonIgnore] public string Detail => $"{(UiStrings.Language == "es" ? "Guardada" : "Saved")}: {SavedUtc.ToLocalTime():dd/MM/yyyy HH:mm}";
    [JsonIgnore] public string LoginLabel => BuildLoginLabel(ProfileSource);
    [JsonIgnore] public ImageSource? IconImage => IconKind == "custom" ? ProfileIcons.Load(Id) : BuiltinIcons.SourceFor(IconKind);
    public event PropertyChangedEventHandler? PropertyChanged;

    private static string BuildLoginLabel(string? source)
    {
        source = (source ?? "").Trim();
        if (string.IsNullOrWhiteSpace(source)) return string.Empty;
        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri)) return source;
        string tail = uri.Segments.LastOrDefault()?.Trim('/').Trim() ?? string.Empty;
        tail = Uri.UnescapeDataString(tail);
        if (string.IsNullOrWhiteSpace(tail)) return source;
        int dash = tail.LastIndexOf('-');
        if (dash > 0 && dash < tail.Length - 1)
            return tail[..dash] + "#" + tail[(dash + 1)..];
        return tail;
    }
    public void Refresh()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Alias)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detail)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoginLabel)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconImage)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IconKind)));
    }
}

