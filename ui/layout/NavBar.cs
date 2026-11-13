

using Avalonia.Controls;

namespace penelope.layout;

public class NavBar
{
    public static Menu Create(MainWindow owner)
    {
        var settings = new MenuItem { Header = "_Settings..." };
        settings.Click += (_, _) => owner.NavigateSettings();

        var exit = new MenuItem { Header = "E_xit" };
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
                        new MenuItem { Header = "_Open Project..." },
                        settings,
                        new Separator(),
                        exit,
                    }
                },
                new MenuItem { Header = "_View" },
            }
        };
    }
}