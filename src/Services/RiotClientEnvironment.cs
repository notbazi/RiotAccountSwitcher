using System.Diagnostics;
using System.IO;

namespace RiotSwitcherAccount;

/// <summary>Finds the official Riot Client and checks it is not still running.</summary>
internal static class RiotClientEnvironment
{
    public static string DetectLauncher()
    {
        string[] paths = [
            @"C:\Riot Games\Riot Client\RiotClientServices.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Riot Games", "Riot Client", "RiotClientServices.exe")
        ];
        return paths.FirstOrDefault(File.Exists) ?? paths[0];
    }

    public static bool ClientIsClosed(out string runningName)
    {
        string[] names = ["RiotClientServices", "RiotClientUx", "RiotClientUxRender", "LeagueClient", "LeagueClientUx", "LeagueClientUxRender", "League of Legends", "VALORANT", "VALORANT-Win64-Shipping"];
        foreach (var name in names)
        {
            if (Process.GetProcessesByName(name).Length > 0) { runningName = name; return false; }
        }
        runningName = "";
        return true;
    }

}
