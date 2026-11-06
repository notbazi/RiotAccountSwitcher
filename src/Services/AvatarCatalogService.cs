using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RiotSwitcherAccount;

/// <summary>
/// Opt-in, content-only avatar catalogs. No Riot IDs, profile lookups, login details or scraping.
/// LoL: official Data Dragon. VALORANT: unofficial Valorant-API game content catalog.
/// Remote game artwork is NOT bundled in the program; usage rights remain with its owners.
/// </summary>
internal static class AvatarCatalogService
{
    private const int MaxJson = 7_000_000;
    private const int MaxImage = 1_500_000;
    private static readonly HttpClient Client = NewClient();

    private static HttpClient NewClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(14) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("RiotAccountSwitcher/0.7 (opt-in public game asset catalog)");
        return client;
    }

    private static async Task<byte[]> GetLimitedAsync(Uri uri, int sizeLimit, string contentType, CancellationToken token)
    {
        if (uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443 || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidDataException("Invalid catalog address.");
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        response.EnsureSuccessStatusCode();
        string mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (!mediaType.Contains(contentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unexpected catalog response format.");
        if (response.Content.Headers.ContentLength is long contentLength && contentLength > sizeLimit)
            throw new InvalidDataException("Catalog response too large.");
        using var stream = await response.Content.ReadAsStreamAsync(token);
        using var memory = new MemoryStream();
        byte[] buffer = new byte[16_384];
        int read;
        while ((read = await stream.ReadAsync(buffer, token)) != 0)
        {
            if (memory.Length + read > sizeLimit) throw new InvalidDataException("Catalog response too large.");
            memory.Write(buffer, 0, read);
        }
        return memory.ToArray();
    }

    private static async Task<JsonDocument> FetchJsonAsync(string url, string exactHost, CancellationToken token)
    {
        var uri = new Uri(url);
        if (!uri.Host.Equals(exactHost, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Unexpected catalog host.");
        byte[] data = await GetLimitedAsync(uri, MaxJson, "json", token);
        return JsonDocument.Parse(data, new JsonDocumentOptions { MaxDepth = 18 });
    }

    public static async Task<IReadOnlyList<AvatarOption>> FetchLeagueAsync(CancellationToken token)
    {
        using var versions = await FetchJsonAsync("https://ddragon.leagueoflegends.com/api/versions.json", "ddragon.leagueoflegends.com", token);
        string version = versions.RootElement[0].GetString() ?? throw new InvalidDataException("No Data Dragon version.");
        if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$")) throw new InvalidDataException("Invalid Data Dragon version.");
        string url = $"https://ddragon.leagueoflegends.com/cdn/{version}/data/en_US/profileicon.json";
        using var json = await FetchJsonAsync(url, "ddragon.leagueoflegends.com", token);
        var results = new List<(int Id, AvatarOption Entry)>();
        if (!json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Data Dragon catalog format changed.");
        foreach (var icon in data.EnumerateObject())
        {
            if (!int.TryParse(icon.Name, out int id) || id is <= 0 or > 100000000) continue;
            string picture = $"https://ddragon.leagueoflegends.com/cdn/{version}/img/profileicon/{id}.png";
            results.Add((id, new AvatarOption($"#{id}", picture)));
        }
        // Highest IDs often include recently introduced icons. Searching by ID remains available.
        return results.OrderByDescending(x => x.Id).Select(x => x.Entry).ToList();
    }

    public static async Task<IReadOnlyList<AvatarOption>> FetchValorantAsync(CancellationToken token)
    {
        using var json = await FetchJsonAsync("https://valorant-api.com/v1/playercards?language=en-US", "valorant-api.com", token);
        if (!json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("VALORANT cards catalog format changed.");
        var options = new List<AvatarOption>();
        foreach (var entry in data.EnumerateArray())
        {
            if (!entry.TryGetProperty("displayName", out var label) || label.ValueKind != JsonValueKind.String ||
                !entry.TryGetProperty("smallArt", out var art) || art.ValueKind != JsonValueKind.String)
                continue;
            string? name = label.GetString();
            string? url = art.GetString();
            if (name is null || url is null || !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                !AllowedImage(uri)) continue;
            options.Add(new AvatarOption(name, url));
        }
        return options.OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool AllowedImage(Uri uri)
    {
        if (uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443 || !string.IsNullOrEmpty(uri.UserInfo)) return false;
        if (uri.Host.Equals("ddragon.leagueoflegends.com", StringComparison.OrdinalIgnoreCase))
            return Regex.IsMatch(uri.AbsolutePath, @"^/cdn/\d+\.\d+\.\d+/img/profileicon/\d+\.png$", RegexOptions.IgnoreCase);
        if (uri.Host.Equals("media.valorant-api.com", StringComparison.OrdinalIgnoreCase))
            return Regex.IsMatch(uri.AbsolutePath, @"^/playercards/[a-f0-9-]{36}/[a-z0-9-]+\.(png|jpg|jpeg)$", RegexOptions.IgnoreCase);
        return false;
    }

    public static async Task<byte[]> FetchImageAsync(string url, CancellationToken token)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !AllowedImage(uri))
            throw new InvalidDataException("Image address is not on a permitted game asset host.");
        return await GetLimitedAsync(uri, MaxImage, "image/", token);
    }

    public static ImageSource ImageFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes, false);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DecodePixelWidth = 170;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}

internal sealed class AvatarOption : System.ComponentModel.INotifyPropertyChanged
{
    private ImageSource? _thumbnail;
    public string Label { get; }
    public string? ImageUrl { get; }
    public string? BuiltinKind { get; }
    public byte[]? DownloadedBytes { get; set; }
    public ImageSource? Thumbnail
    {
        get => _thumbnail;
        set { _thumbnail = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Thumbnail))); }
    }
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    public AvatarOption(string label, string imageUrl) { Label = label; ImageUrl = imageUrl; }
    public AvatarOption(string label, string builtInKind, ImageSource? thumbnail)
    { Label = label; BuiltinKind = builtInKind; Thumbnail = thumbnail; }
}
