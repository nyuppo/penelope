using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
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

        var menu = NavBar.Create(this);
        menu.Margin = new Thickness(16, 0);
        menu.VerticalAlignment = VerticalAlignment.Center;

        var navbar = new Border
        {
            Classes = { "nav" },
            Child = new Panel
            {
                Children =
                {
                    new LinenTexture(),
                    new Line
                    {
                        Classes = { "stitch" },
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 0),
                        Stretch = Stretch.Fill,
                    },
                    menu,
                }
            }
        };
        DockPanel.SetDock(navbar, Dock.Top);

        Content = new DockPanel { Children = { navbar, _outlet } };

        NavigateHome();
    }

    public void Navigate(Control page) => _outlet.Content = page;

    public void NavigateHome() => Navigate(new HomeView(_config));
    public void NavigateSettings() => Navigate(new SettingsView(_config, onDone: NavigateHome));
}