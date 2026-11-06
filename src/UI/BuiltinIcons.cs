using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RiotSwitcherAccount;

internal static class BuiltinIcons
{
    public static ImageSource? SourceFor(string? kind)
    {
        string? uri = kind?.ToLowerInvariant() switch
        {
            "valorant" => "pack://application:,,,/Assets/avatar-valorant.png",
            "league" => "pack://application:,,,/Assets/avatar-league.png",
            "tft" => "pack://application:,,,/Assets/avatar-tft.png",
            _ => "pack://application:,,,/Assets/avatar-neutral.png"
        };
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.DecodePixelWidth = 96;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch { return null; }
    }
}
