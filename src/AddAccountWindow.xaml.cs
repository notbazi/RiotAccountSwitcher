using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace RiotSwitcherAccount;

/// <summary>Only adds new Riot Client sessions. Existing accounts and settings live in the main window.</summary>
public partial class AddAccountWindow : Window
{
    private readonly SessionStore _store = new();
    private readonly DispatcherTimer _noticeTimer = new() { Interval = TimeSpan.FromSeconds(10) };
    private Preferences _settings = new();
    private byte[]? _pendingAvatarImage;
    private string? _pendingBuiltinAvatar;

    public AddAccountWindow()
    {
        InitializeComponent();
        UpdateGameFields();
        _noticeTimer.Tick += (_, _) =>
        {
            _noticeTimer.Stop();
            NoticeText.Text = "";
            NoticeText.Visibility = Visibility.Collapsed;
        };
        try
        {
            _settings = _store.Load();
            UiStrings.ApplyLanguage(_settings.Language);
            ThemeAccent.Apply(_settings.AccentHex);
            if (!File.Exists(_settings.RiotExe))
                _settings.RiotExe = RiotClientEnvironment.DetectLauncher();
        }
        catch (Exception ex)
        {
            DialogService.Error(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T("ConfigError") + "\n\n" + ex.Message);
        }
    }

    private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            try { DragMove(); } catch (InvalidOperationException) { }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void GameBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateGameFields();

    private void UpdateGameFields()
    {
        // SelectionChanged fires during InitializeComponent, before other controls exist.
        if (GameBox is null || ProfileFieldsPanel is null) return;
        string game = (GameBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "valorant";
        ProfileFieldsPanel.Visibility = game == "riot" ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ChooseAvatar_Click(object sender, RoutedEventArgs e)
    {
        string game = (GameBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "valorant";
        var gallery = new AvatarGalleryWindow(game) { Owner = this };
        if (gallery.ShowDialog() != true) return;
        _pendingAvatarImage = gallery.SelectedImageBytes;
        _pendingBuiltinAvatar = gallery.SelectedBuiltinKind;
        AvatarStatusText.Text = UiStrings.T("AvatarSelected");
        AvatarStatusText.Visibility = Visibility.Visible;
    }

    private void CustomAvatar_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            CheckFileExists = true,
            Title = UiStrings.T("ImagePicker")
        };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            var image = new FileInfo(dialog.FileName);
            if (image.Length is < 1 or > 1_000_000)
                throw new InvalidDataException(UiStrings.T("IconBad"));
            _pendingAvatarImage = File.ReadAllBytes(dialog.FileName);
            _pendingBuiltinAvatar = null;
            AvatarStatusText.Text = UiStrings.T("AvatarSelected");
            AvatarStatusText.Visibility = Visibility.Visible;
        }
        catch (Exception ex) { DialogService.Info(this, UiStrings.T("IconError"), ex.Message); }
    }

    private void SetStatus(string key, params object[] args)
    {
        _noticeTimer.Stop();
        NoticeText.Text = UiStrings.F(key, args);
        NoticeText.Visibility = Visibility.Visible;
        _noticeTimer.Start();
    }

    private void ShowError(string key, Exception ex)
        => DialogService.Error(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T(key) + "\n\n" + ex.Message);

    private bool CheckClosed()
    {
        try
        {
            if (RiotClientEnvironment.ClientIsClosed(out var running)) return true;
            DialogService.Info(this, UiStrings.T("CloseRiotTitle"), UiStrings.F("CloseRiotText", running));
        }
        catch (Exception ex) { ShowError("ClientCheckError", ex); }
        return false;
    }

    private void Capture_Click(object sender, RoutedEventArgs e)
    {
        if (!CheckClosed()) return;
        string alias = AliasBox.Text.Trim();
        if (alias.Length is < 2 or > 40)
        {
            DialogService.Info(this, UiStrings.T("MissingAliasTitle"), UiStrings.T("MissingAlias"));
            return;
        }
        if (_settings.Profiles.Any(p => string.Equals(p.Alias, alias, StringComparison.OrdinalIgnoreCase)))
        {
            DialogService.Info(this, UiStrings.T("DuplicateTitle"), UiStrings.T("DuplicateAlias"));
            return;
        }
        SaveSessionButton.IsEnabled = false;
        try
        {
            var sessionFile = _store.CurrentFile();
            if (!File.Exists(sessionFile)) throw new InvalidOperationException(UiStrings.T("MissingSession"));
            byte[] raw = File.ReadAllBytes(sessionFile);
            if (!SessionStore.LooksAuthenticated(raw)) throw new InvalidOperationException(UiStrings.T("NoAuthSession"));
            string game = (GameBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "riot";
            string source = game == "riot" ? string.Empty : RiotIdBox.Text.Trim();
            var profile = new Profile
            {
                Alias = alias, Filename = Path.GetFileName(sessionFile), IconKind = _pendingBuiltinAvatar ?? game,
                GameKind = game, ProfileSource = source
            };
            _store.SaveSnapshot(profile, raw);
            _settings.Profiles.Add(profile);
            _settings.ActiveId = profile.Id;
            _store.Save(_settings);
            SetStatus("SavedStatus", alias);

            // Avatar import is optional and never rolls back a successfully saved session.
            if (_pendingAvatarImage is not null)
            {
                try
                {
                    ProfileIcons.ImportDownloaded(profile.Id, _pendingAvatarImage);
                    profile.IconKind = "custom";
                    _store.Save(_settings);
                }
                catch { DialogService.Info(this, UiStrings.T("AddSuccessTitle"), UiStrings.T("AvatarSaveFailed")); }
            }
            DialogResult = true;
        }
        catch (Exception ex) { ShowError("CaptureError", ex); }
        finally { SaveSessionButton.IsEnabled = true; }
    }

    private void FreshLogin_Click(object sender, RoutedEventArgs e)
    {
        if (!CheckClosed()) return;
        if (!DialogService.Confirm(this, UiStrings.T("NewLoginTitle"), UiStrings.T("NewLoginQuestion"))) return;
        try
        {
            string currentPath = _store.CurrentFile();
            byte[]? old = File.Exists(currentPath) ? File.ReadAllBytes(currentPath) : null;
            if (old is not null)
            {
                _store.SaveRecovery(Path.GetFileName(currentPath), old);
                if (_settings.ActiveId is not null)
                {
                    Profile? active = _settings.Profiles.FirstOrDefault(p => p.Id == _settings.ActiveId);
                    if (active is not null && SessionStore.LooksAuthenticated(old)) _store.SaveSnapshot(active, old);
                }
                File.Delete(currentPath);
            }
            string? previousId = _settings.ActiveId;
            _settings.ActiveId = null;
            _store.Save(_settings);
            try { LaunchRiotClient(); }
            catch
            {
                if (old is not null) _store.WriteSession(Path.GetFileName(currentPath), old);
                _settings.ActiveId = previousId;
                _store.Save(_settings);
                throw;
            }
            SetStatus("FreshStatus");
            // Keep the add-account window open so the user can save the new session.
        }
        catch (Exception ex) { ShowError("FreshError", ex); }
    }

    private void LaunchRiotClient()
    {
        string path = _settings.RiotExe.Trim().Trim('"');
        if (!File.Exists(path)) throw new FileNotFoundException(UiStrings.T("LauncherNotFound"), path);
        if (!string.Equals(Path.GetFileName(path), "RiotClientServices.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(UiStrings.T("LauncherWrong"));
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = path, WorkingDirectory = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory,
            UseShellExecute = true
        });
        if (process is null) throw new InvalidOperationException(UiStrings.T("LauncherStartError"));
    }
}
