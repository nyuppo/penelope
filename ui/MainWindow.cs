using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using penelope.layout;

namespace penelope;

public class MainWindow : Window
{
    readonly GlobalConfig _config;
    readonly ContentControl _outlet = new();

    public MainWindow(GlobalConfig config)
    {
        _config = config;

        Title = "Penelope";
        Width = 1100;
        Height = 700;

        var navbar = new Border
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = NavBar.Create(this),
        };
        DockPanel.SetDock(navbar, Dock.Top);

        Content = new DockPanel { Children = { navbar, _outlet } };

        NavigateHome();
    }

    public void Navigate(Control page) => _outlet.Content = page;

    public void NavigateHome() => Navigate(new HomeView(_config));
    public void NavigateSettings() => Navigate(new SettingsView(_config, onDone: NavigateHome));
}