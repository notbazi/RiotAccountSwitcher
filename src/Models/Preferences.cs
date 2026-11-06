namespace RiotSwitcherAccount;

public sealed class Preferences
{
    public List<Profile> Profiles { get; set; } = new();
    public string? ActiveId { get; set; }
    public string RiotExe { get; set; } = @"C:\Riot Games\Riot Client\RiotClientServices.exe";
    public bool CloseOnOpen { get; set; } = true;
    public bool LauncherPathLocked { get; set; }
    public string Language { get; set; } = "en";
    public string AccentHex { get; set; } = ThemeAccent.Default;
}

