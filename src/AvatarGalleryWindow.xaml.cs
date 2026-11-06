using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RiotSwitcherAccount;

/// <summary>User-initiated avatar selection; does not affect any Riot Client sessions.</summary>
public partial class AvatarGalleryWindow : Window
{
    private readonly ObservableCollection<AvatarOption> _visible = new();
    private IReadOnlyList<AvatarOption> _entries = Array.Empty<AvatarOption>();
    private readonly Dictionary<string, IReadOnlyList<AvatarOption>> _catalogCache = new();
    private CancellationTokenSource? _pageCancellation;
    private int _page;
    private string _provider = "local";
    private bool _ready;
    private const int PageSize = 16;

    public string? SelectedBuiltinKind { get; private set; }
    public byte[]? SelectedImageBytes { get; private set; }

    public AvatarGalleryWindow(string? preferredGame = null)
    {
        InitializeComponent();
        AvatarTiles.ItemsSource = _visible;
        _ready = true;
        // Selection is intentionally offline by default; network is opt-in.
        ShowLocal();
    }

    private void ShowLocal()
    {
        _pageCancellation?.Cancel();
        _provider = "local";
        _entries = new[]
        {
            new AvatarOption("VALORANT", "valorant", BuiltinIcons.SourceFor("valorant")),
            new AvatarOption("League of Legends", "league", BuiltinIcons.SourceFor("league")),
            new AvatarOption("Teamfight Tactics", "tft", BuiltinIcons.SourceFor("tft")),
            new AvatarOption("Neutral", "riot", BuiltinIcons.SourceFor("riot"))
        };
        SearchBox.Text = string.Empty;
        _page = 0;
        SourceNote.Text = UiStrings.T("GalleryLocalNote");
        StatusText.Text = string.Empty;
        ShowPage();
    }

    private async Task SetOnlineAsync(string provider)
    {
        _pageCancellation?.Cancel();
        _provider = provider;
        SourceNote.Text = provider == "league" ? UiStrings.T("GalleryLeagueNotice") : UiStrings.T("GalleryValorantNotice");
        SearchBox.Text = string.Empty;
        StatusText.Text = UiStrings.T("GalleryLoading");
        _visible.Clear();
        PreviousPage.IsEnabled = NextPage.IsEnabled = false;
        try
        {
            if (!_catalogCache.TryGetValue(provider, out var entries))
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                entries = provider == "league"
                    ? await AvatarCatalogService.FetchLeagueAsync(cts.Token)
                    : await AvatarCatalogService.FetchValorantAsync(cts.Token);
                _catalogCache[provider] = entries;
            }
            if (provider != _provider) return;
            _entries = entries;
            _page = 0;
            StatusText.Text = string.Empty;
            ShowPage();
        }
        catch (Exception ex)
        {
            if (provider != _provider) return;
            _entries = Array.Empty<AvatarOption>();
            StatusText.Text = UiStrings.T("GalleryUnavailable") + " " + ex.Message;
            ShowPage();
        }
    }

    private void ShowPage()
    {
        if (!_ready) return;
        _pageCancellation?.Cancel();
        _pageCancellation?.Dispose();
        _pageCancellation = new CancellationTokenSource();
        var token = _pageCancellation.Token;
        string query = SearchBox.Text.Trim();
        var filtered = _entries.Where(x => x.Label.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        int pages = Math.Max(1, (filtered.Count + PageSize - 1) / PageSize);
        _page = Math.Clamp(_page, 0, pages - 1);
        _visible.Clear();
        var pageItems = filtered.Skip(_page * PageSize).Take(PageSize).ToArray();
        foreach (var item in pageItems) _visible.Add(item);
        CatalogCount.Text = filtered.Count.ToString() + " " + UiStrings.T("GalleryResults");
        PageLabel.Text = $"{_page + 1}/{pages}";
        PreviousPage.IsEnabled = _page > 0;
        NextPage.IsEnabled = _page + 1 < pages;
        if (_provider != "local") _ = PopulateThumbnailsAsync(pageItems, token);
    }

    private async Task PopulateThumbnailsAsync(AvatarOption[] items, CancellationToken token)
    {
        using var throttle = new SemaphoreSlim(4);
        await Task.WhenAll(items.Select(async item =>
        {
            if (item.Thumbnail is not null || item.ImageUrl is null) return;
            try
            {
                await throttle.WaitAsync(token);
                try
                {
                    byte[] bytes = await AvatarCatalogService.FetchImageAsync(item.ImageUrl, token);
                    if (token.IsCancellationRequested) return;
                    ImageSource picture = AvatarCatalogService.ImageFromBytes(bytes);
                    if (token.IsCancellationRequested) return;
                    item.DownloadedBytes = bytes;
                    item.Thumbnail = picture;
                }
                finally { throttle.Release(); }
            }
            catch (OperationCanceledException) { }
            catch { /* Bad/missing thumbnail does not block other items. */ }
        }));
    }

    private async void Choose_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: AvatarOption option }) return;
        if (option.BuiltinKind is string builtIn)
        {
            SelectedBuiltinKind = builtIn;
            DialogResult = true;
            return;
        }
        if (option.ImageUrl is null) return;
        if (sender is Button button) button.IsEnabled = false;
        StatusText.Text = UiStrings.T("GalleryLoading");
        try
        {
            var data = option.DownloadedBytes ?? await AvatarCatalogService.FetchImageAsync(option.ImageUrl, CancellationToken.None);
            // Decoder validates format and pixel counts at import time, before saving the avatar.
            SelectedImageBytes = data;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = UiStrings.T("GalleryUnavailable") + " " + ex.Message;
        }
        finally { if (sender is Button b) b.IsEnabled = true; }
    }

    private void Local_Click(object sender, RoutedEventArgs e) => ShowLocal();
    private async void League_Click(object sender, RoutedEventArgs e) => await SetOnlineAsync("league");
    private async void Valorant_Click(object sender, RoutedEventArgs e) => await SetOnlineAsync("valorant");
    private void Search_TextChanged(object sender, TextChangedEventArgs e) { if (_ready) { _page = 0; ShowPage(); } }
    private void Previous_Click(object sender, RoutedEventArgs e) { if (_page > 0) { _page--; ShowPage(); } }
    private void Next_Click(object sender, RoutedEventArgs e) { _page++; ShowPage(); }
    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            try { DragMove(); } catch (InvalidOperationException) { }
    }
    protected override void OnClosed(EventArgs e)
    {
        _pageCancellation?.Cancel();
        _pageCancellation?.Dispose();
        base.OnClosed(e);
    }
}
