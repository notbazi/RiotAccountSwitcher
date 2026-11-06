using System.Windows;
using System.Windows.Input;

namespace RiotSwitcherAccount;

public partial class RenameDialog : Window
{
    public string AccountName => NameBox.Text.Trim();

    public RenameDialog(string currentName)
    {
        InitializeComponent();
        NameBox.Text = currentName;
        Loaded += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
        PreviewKeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Save_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                e.Handled = true;
            }
        };
    }

    private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch (InvalidOperationException) { }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (AccountName.Length is < 2 or > 40)
        {
            ValidationText.Visibility = Visibility.Visible;
            NameBox.Focus();
            return;
        }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
