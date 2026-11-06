using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace RiotSwitcherAccount;

public partial class SettingsWindow : Window
{
    private readonly Preferences _preferences;
    private readonly Action _persist;
    private readonly SessionStore _store = new();
    private bool _initializing = true;

    public SettingsWindow(Preferences preferences, Action persist)
    {
        InitializeComponent();
        _preferences = preferences;
        _persist = persist;
        RiotPathBox.Text = preferences.RiotExe;
        AutoCloseBox.IsChecked = preferences.CloseOnOpen;
        DrawColors();
        UpdateSelectedLanguage();
        _initializing = false;
    }

    private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) { try { DragMove(); } catch (InvalidOperationException) { } }
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void UpdateSelectedLanguage()
    {
        var neutral = new SolidColorBrush(Color.FromRgb(42, 62, 91));
        var selected = new SolidColorBrush(Color.FromRgb(58, 86, 124));
        EnglishButton.Background = _preferences.Language == "es" ? neutral : selected;
        SpanishButton.Background = _preferences.Language == "es" ? selected : neutral;
    }

    private void Language_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.Tag is not string lang) return;
        _preferences.Language = lang;
        UiStrings.ApplyLanguage(lang);
        UpdateSelectedLanguage();
        DrawColors();
        SaveChanged();
    }

    private void DrawColors()
    {
        AccentChoices.Children.Clear();
        foreach (var (color, nameKey) in ThemeAccent.Colors)
        {
            var button = new Button
            {
                Width = 42, Height = 42, Margin = new Thickness(0,0,9,0), Tag = color,
                Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                ToolTip = UiStrings.T(nameKey)
            };
            var grid = new Grid();
            grid.Children.Add(new Ellipse
            {
                Width = 28, Height = 28, HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color))
            });
            if (string.Equals(color, _preferences.AccentHex, StringComparison.OrdinalIgnoreCase))
                grid.Children.Add(new Ellipse
                {
                    Width = 39, Height = 39, HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)), StrokeThickness = 2
                });
            button.Content = grid;
            button.Click += Accent_Click;
            AccentChoices.Children.Add(button);
        }
    }

    private void Accent_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.Tag is not string hex) return;
        _preferences.AccentHex = hex;
        ThemeAccent.Apply(hex);
        UpdateSelectedLanguage();
        DrawColors();
        SaveChanged();
    }

    private void ChangePath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Riot Client Services|RiotClientServices.exe|Executable (*.exe)|*.exe",
            FileName = "RiotClientServices.exe", Title = UiStrings.T("LauncherChoose")
        };
        if (dialog.ShowDialog(this) != true) return;
        if (!string.Equals(System.IO.Path.GetFileName(dialog.FileName), "RiotClientServices.exe", StringComparison.OrdinalIgnoreCase))
        {
            DialogService.Info(this, UiStrings.T("LauncherWrongTitle"), UiStrings.T("LauncherWrong"));
            return;
        }
        _preferences.RiotExe = dialog.FileName;
        _preferences.LauncherPathLocked = true;
        RiotPathBox.Text = dialog.FileName;
        SaveChanged();
    }

    private void SaveChanged()
    {
        try { _persist(); }
        catch (Exception ex)
        {
            DialogService.Error(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T("SavePrefError") + "\n\n" + ex.Message);
        }
    }

    private void AutoCloseBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_initializing) return;
        _preferences.CloseOnOpen = AutoCloseBox.IsChecked == true;
        SaveChanged();
    }

    private void Recovery_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!RiotClientEnvironment.ClientIsClosed(out string running))
            {
                DialogService.Info(this, UiStrings.T("CloseRiotTitle"), UiStrings.F("CloseRiotText", running));
                return;
            }
            if (!File.Exists(_store.RecoveryPath))
            {
                DialogService.Info(this, UiStrings.T("NoRecoveryTitle"), UiStrings.T("NoRecovery"));
                return;
            }
            if (!DialogService.Confirm(this, UiStrings.T("RecoveryTitle"), UiStrings.T("RecoveryQuestion"))) return;
            if (!_store.RestoreRecovery()) throw new InvalidOperationException(UiStrings.T("RecoveryInvalid"));
            _preferences.ActiveId = null;
            SaveChanged();
            DialogService.Info(this, UiStrings.T("RecoveryTitle"), UiStrings.T("RecoveryStatus"));
        }
        catch (Exception ex) { DialogService.Error(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T("RecoveryError") + "\n\n" + ex.Message); }
    }

    private void LaunchOnly_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string path = _preferences.RiotExe.Trim().Trim('"');
            if (!File.Exists(path)) throw new FileNotFoundException(UiStrings.T("LauncherNotFound"), path);
            if (!string.Equals(System.IO.Path.GetFileName(path), "RiotClientServices.exe", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(UiStrings.T("LauncherWrong"));
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = System.IO.Path.GetDirectoryName(path) ?? Environment.CurrentDirectory,
                UseShellExecute = true
            });
            if (process is null) throw new InvalidOperationException(UiStrings.T("LauncherStartError"));
            DialogService.Info(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T("LaunchStatus"));
        }
        catch (Exception ex) { DialogService.Error(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T("LaunchError") + "\n\n" + ex.Message); }
    }

    private void Instagram_Click(object sender, MouseButtonEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("https://www.instagram.com/notbazi/") { UseShellExecute = true }); }
        catch (Exception ex)
        {
            DialogService.Error(this, UiStrings.T("GeneralErrorTitle"), UiStrings.T("InstagramError") + "\n" + ex.Message);
        }
    }
}
