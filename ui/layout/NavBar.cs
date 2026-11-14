

using System.IO;
using System.Linq;
using Avalonia.Controls;

namespace penelope.layout;

public class NavBar
{
    private static MenuItem StyledNavTab(string header) => new MenuItem { Header = header, Classes = { "tab" } };

    public static Menu Create(MainWindow owner)
    {
        var newProject = StyledNavTab("_New Project...");
        newProject.Click += async (_, _) => await owner.CreateNewProject();

        var openExistingProject = StyledNavTab("_Open Project...");
        openExistingProject.Click += async (_, _) => await owner.OpenExistingProject();

        var recentProjects = StyledNavTab("_Recent Projects");

        var settings = StyledNavTab("_Settings...");
        settings.Click += (_, _) => owner.NavigateSettings();

        var exit = StyledNavTab("E_xit");
        exit.Click += (_, _) => owner.Close();

        var file = new MenuItem
        {
            Header = "_File",
            Items =
            {
                newProject,
                openExistingProject,
                recentProjects,
                new Separator(),
                settings,
                new Separator(),
                exit,
            }
        };

        file.SubmenuOpened += (_, e) =>
        {
            if (e.Source == file)
            {
                PopulateRecents(recentProjects, owner);
            };
        };
        PopulateRecents(recentProjects, owner);

        return new Menu
        {
            Items =
            {
                file,
            }
        };
    }

    static void PopulateRecents(MenuItem recentMenu, MainWindow owner)
    {
        recentMenu.Items.Clear();

        foreach (var path in owner.RecentProjects.ToList())
        {
            // escape underscores to avoid _ being treated as access key
            var name = Path.GetFileNameWithoutExtension(path).Replace("_", "__");

            var item = StyledNavTab(name);
            ToolTip.SetTip(item, path);
            item.Click += async (_, _) => await owner.OpenProject(path);
            recentMenu.Items.Add(item);
        }

        recentMenu.IsEnabled = recentMenu.Items.Count > 0;
    }
}