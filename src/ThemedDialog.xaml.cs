using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RiotSwitcherAccount;

public partial class ThemedDialog : Window
{
    public ThemedDialog(string title, string message, bool isQuestion = false, bool destructive = false)
    {
        InitializeComponent();
        DialogTitle.Text = title;
        DialogMessage.Text = message;
        CancelButton.Content = UiStrings.T("Cancel");
        OkButton.Content = isQuestion ? UiStrings.T("Confirm") : UiStrings.T("Okay");
        CancelButton.Visibility = isQuestion ? Visibility.Visible : Visibility.Collapsed;
        if (destructive)
        {
            OkButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"));
            OkButton.Foreground = Brushes.White;
            DialogGlyph.Foreground = OkButton.Background;
            DialogGlyph.Text = "";
        }
        else if (isQuestion)
        {
            DialogGlyph.Text = "";
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

internal static class DialogService
{
    public static void Info(Window owner, string title, string message)
    {
        var dlg = new ThemedDialog(title, message) { Owner = owner };
        dlg.ShowDialog();
    }

    public static void Error(Window owner, string title, string message)
    {
        var dlg = new ThemedDialog(title, message) { Owner = owner };
        dlg.ShowDialog();
    }

    public static bool Confirm(Window owner, string title, string message, bool destructive = false)
    {
        var dlg = new ThemedDialog(title, message, isQuestion: true, destructive: destructive) { Owner = owner };
        return dlg.ShowDialog() == true;
    }
}
