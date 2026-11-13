

using Avalonia.Controls;

namespace penelope.layout;

public class NavBar
{
    private static MenuItem StyledNavTab(string header) => new MenuItem { Header = header, Classes = { "tab" } };

    public static Menu Create(MainWindow owner)
    {
        var settings = StyledNavTab("_Settings...");
        settings.Click += (_, _) => owner.NavigateSettings();

        var exit = StyledNavTab("E_xit");
        exit.Click += (_, _) => owner.Close();

        return new Menu
        {
            Items =
            {
                new MenuItem
                {
                    Header = "_File",
                    Items =
                    {
                        StyledNavTab("Open Project..."),
                        settings,
                        new Separator(),
                        exit,
                    }
                },
                StyledNavTab("_View")
            }
        };
    }
}