using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace RiotSwitcherAccount;

/// <summary>
/// Compact, Steam-inspired account chooser. It delegates all session switching
/// to AccountSwitchService and keeps account management in a separate window.
/// </summary>
public partial class MainWindow : Window
{
    private readonly SessionStore _store = new();
    private readonly ObservableCollection<Profile> _accounts = new();
    private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromSeconds(10) };
    private Preferences _preferences = new();
    private Profile? _contextProfile;

    public MainWindow()
    {
        InitializeComponent();
        AccountTiles.ItemsSource = _accounts;
        _statusTimer.Tick += (_, _) =>
        {
            _statusTimer.Stop();
            NoticeText.Text = string.Empty;
            NoticeText.Visibility = Visibility.Collapsed;
        };
        LoadProfiles();
    }

    private static string T(string key) => UiStrings.T(key);

    private void LoadProfiles()
    {
        try
        {
            _preferences = _store.Load();
            UiStrings.ApplyLanguage(_preferences.Language);
            ThemeAccent.Apply(_preferences.AccentHex);
            if (!File.Exists(_preferences.RiotExe))
            {
                _preferences.RiotExe = RiotClientEnvironment.DetectLauncher();
                _store.Save(_preferences);
            }
            _accounts.Clear();
            foreach (var profile in _preferences.Profiles)
            {
                profile.Refresh();
                _accounts.Add(profile);
            }
        }
        catch (Exception ex) { DialogService.Error(this, T("GeneralErrorTitle"), T("ConfigError") + "\n\n" + ex.Message); }
    }

    private void SetStatus(string message)
    {
        _statusTimer.Stop();
        NoticeText.Text = message;
        NoticeText.Visibility = string.IsNullOrWhiteSpace(message) ? Visibility.Collapsed : Visibility.Visible;
        if (NoticeText.Visibility == Visibility.Visible) _statusTimer.Start();
    }

    private bool RiotClientClosed()
    {
        try
        {
            if (RiotClientEnvironment.ClientIsClosed(out var process)) return true;
            DialogService.Info(this, T("CloseRiotTitle"), UiStrings.F("CloseRiotText", process));
        }
        catch (Exception ex) { DialogService.Error(this, T("GeneralErrorTitle"), T("ClientCheckError") + "\n\n" + ex.Message); }
        return false;
    }

    private void Profile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Profile profile } || !RiotClientClosed()) return;
        try
        {
            AccountSwitchService.Switch(_store, _preferences, profile);
            if (_preferences.CloseOnOpen) Close();
            else SetStatus(UiStrings.F("SwitchStatus", profile.Alias));
        }
        catch (Exception ex)
        {
            DialogService.Error(this, T("GeneralErrorTitle"), T("SwitchError") + "\n\n" + ex.Message);
        }
    }

    private void AddAccount_Click(object sender, RoutedEventArgs e)
    {
        var addAccount = new AddAccountWindow { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        addAccount.ShowDialog();
        LoadProfiles();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settings = new SettingsWindow(_preferences, () =>
        {
            _store.Save(_preferences);
            UiStrings.ApplyLanguage(_preferences.Language);
            ThemeAccent.Apply(_preferences.AccentHex);
            foreach (var profile in _accounts) profile.Refresh();
        }) { Owner = this };
        settings.ShowDialog();
        LoadProfiles();
    }

    private void AccountMenu_Opened(object sender, RoutedEventArgs e)
    {
        _contextProfile = sender is ContextMenu menu
            && menu.PlacementTarget is FrameworkElement element
            ? element.DataContext as Profile
            : null;
    }

    private void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (_contextProfile is not Profile chosen) return;
        var dialog = new RenameDialog(chosen.Alias) { Owner = this };
        if (dialog.ShowDialog() != true) return;
        string alias = dialog.AccountName;
        if (alias == chosen.Alias) return;
        if (_accounts.Any(p => p.Id != chosen.Id && string.Equals(p.Alias, alias, StringComparison.OrdinalIgnoreCase)))
        {
            DialogService.Info(this, T("DuplicateTitle"), T("DuplicateAlias"));
            return;
        }
        string previous = chosen.Alias;
        try
        {
            chosen.Alias = alias;
            _store.Save(_preferences);
            chosen.Refresh();
            SetStatus(T("RenameSaved"));
        }
        catch (Exception ex)
        {
            chosen.Alias = previous;
            chosen.Refresh();
            DialogService.Error(this, T("GeneralErrorTitle"), T("RenameError") + "\n\n" + ex.Message);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_contextProfile is not Profile chosen) return;
        if (!DialogService.Confirm(this, T("DeleteConfirm"), UiStrings.F("DeleteQuestion", chosen.Alias), destructive: true)) return;
        try
        {
            _store.DeleteSnapshot(chosen);
            ProfileIcons.Remove(chosen.Id);
            _preferences.Profiles.Remove(chosen);
            if (_preferences.ActiveId == chosen.Id) _preferences.ActiveId = null;
            _store.Save(_preferences);
            _accounts.Remove(chosen);
            _contextProfile = null;
            SetStatus(T("DeleteStatus"));
        }
        catch (Exception ex) { DialogService.Error(this, T("GeneralErrorTitle"), T("DeleteError") + "\n\n" + ex.Message); }
    }

    private void ValorantIcon_Click(object sender, RoutedEventArgs e) => SetIcon("valorant");
    private void LeagueIcon_Click(object sender, RoutedEventArgs e) => SetIcon("league");
    private void TftIcon_Click(object sender, RoutedEventArgs e) => SetIcon("tft");
    private void RiotIcon_Click(object sender, RoutedEventArgs e) => SetIcon("riot");

    private void SetIcon(string icon)
    {
        if (_contextProfile is not Profile profile) return;
        string oldIcon = profile.IconKind;
        try
        {
            profile.IconKind = icon;
            _store.Save(_preferences);
            profile.Refresh();
            if (oldIcon == "custom") ProfileIcons.Remove(profile.Id);
            SetStatus(T("IconSaved"));
        }
        catch (Exception ex)
        {
            profile.IconKind = oldIcon;
            profile.Refresh();
            DialogService.Error(this, T("GeneralErrorTitle"), T("IconError") + "\n\n" + ex.Message);
        }
    }

    private void DownloadIcon_Click(object sender, RoutedEventArgs e)
    {
        // "Choose avatar" replaces the former profile scraping implementation.
        if (_contextProfile is not Profile profile) return;
        var gallery = new AvatarGalleryWindow(profile.GameKind) { Owner = this };
        if (gallery.ShowDialog() != true) return;
        string oldIcon = profile.IconKind;
        try
        {
            if (gallery.SelectedImageBytes is byte[] image)
            {
                ProfileIcons.ImportDownloaded(profile.Id, image);
                profile.IconKind = "custom";
            }
            else if (gallery.SelectedBuiltinKind is string original)
            {
                profile.IconKind = original;
            }
            _store.Save(_preferences);
            profile.Refresh();
            if (oldIcon == "custom" && profile.IconKind != "custom") ProfileIcons.Remove(profile.Id);
            SetStatus(T("IconSaved"));
        }
        catch (Exception ex)
        {
            profile.IconKind = oldIcon;
            profile.Refresh();
            DialogService.Error(this, T("IconError"), ex.Message);
        }
    }

    private void CustomIcon_Click(object sender, RoutedEventArgs e)
    {
        if (_contextProfile is not Profile profile) return;
        var picker = new OpenFileDialog
        {
            Filter = "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            CheckFileExists = true,
            Title = T("ImagePicker")
        };
        if (picker.ShowDialog(this) != true) return;
        try
        {
            ProfileIcons.Import(profile.Id, picker.FileName);
            profile.IconKind = "custom";
            _store.Save(_preferences);
            profile.Refresh();
            SetStatus(T("IconSaved"));
        }
        catch (Exception ex) { DialogService.Error(this, T("IconError"), T("IconBad") + "\n\n" + ex.Message); }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Resize_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is not Thumb { Tag: string edge } || WindowState != WindowState.Normal) return;
        bool left = edge.Contains("Left", StringComparison.Ordinal);
        bool right = edge.Contains("Right", StringComparison.Ordinal);
        bool top = edge.Contains("Top", StringComparison.Ordinal);
        bool bottom = edge.Contains("Bottom", StringComparison.Ordinal);
        if (left)
        {
            double step = Math.Min(e.HorizontalChange, ActualWidth - MinWidth);
            Width = Math.Max(MinWidth, ActualWidth - step);
            Left += step;
        }
        else if (right) Width = Math.Max(MinWidth, ActualWidth + e.HorizontalChange);
        if (top)
        {
            double step = Math.Min(e.VerticalChange, ActualHeight - MinHeight);
            Height = Math.Max(MinHeight, ActualHeight - step);
            Top += step;
        }
        else if (bottom) Height = Math.Max(MinHeight, ActualHeight + e.VerticalChange);
    }
}
