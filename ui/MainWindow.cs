#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using penelope.layout;

namespace penelope;

public class MainWindow : Window
{
    const int MaxRecentProjects = 10;
    public IReadOnlyList<string> RecentProjects => _config.RecentProjects;

    static readonly FilePickerFileType PenelopeProjectType = new ("Penelope Project")
    {
        Patterns = ["*" + ProjectDefinition.Extension]
    };

    readonly GlobalConfig _config;
    readonly ContentControl _outlet = new();
    Project? _currentProject;

    public MainWindow(GlobalConfig config)
    {
        _config = config;

        Title = "Penelope";
        Width = 1100;
        Height = 700;

        var menu = NavBar.Create(this);
        menu.Margin = new Thickness(4);
        menu.VerticalAlignment = VerticalAlignment.Stretch;

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

        if (TryLoadMostRecentProject() is Project last)
        {
            SetCurrentProject(last);
        } else
        {
            NavigateHome();
        }
    }

    // Navigation
    public void Navigate(Control page) => _outlet.Content = page;

    public void NavigateHome() => Navigate(_currentProject is null
        ? new NoProjectView(this)
        : new HomeView(_config, _currentProject)
    );
    public void NavigateSettings() => Navigate(new SettingsView(_config, onDone: NavigateHome));

    // Projects
    public async Task OpenExistingProject()
    {
        var start = await StorageProvider.TryGetFolderFromPathAsync(_config.EffectiveProjectsRoot);

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Penelope Project",
            SuggestedStartLocation = start,
            AllowMultiple = false,
            FileTypeFilter = [PenelopeProjectType],
        });

        if (files.Count == 0)
        {
            return; // cancelled
        }
        if (files[0].TryGetLocalPath() is not string path)
        {
            return;
        }

        await OpenProject(path);
    }

    public async Task CreateNewProject()
    {
        // ensure default folder exists before trying to create picker there
        Directory.CreateDirectory(_config.EffectiveProjectsRoot);
        var start = await StorageProvider.TryGetFolderFromPathAsync(_config.EffectiveProjectsRoot);

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Create New Penelope Project",
            SuggestedStartLocation = start,
            SuggestedFileName = "Untitled Project",
            DefaultExtension = ProjectDefinition.Extension.TrimStart('.'),
            FileTypeChoices = [PenelopeProjectType],
            ShowOverwritePrompt = false,
        });

        if (file?.TryGetLocalPath() is not string chosenPath)
        {
            return; // cancelled
        }

        // if user tries to create /Documents/MyProject.penelope, we actually want to create /Document/MyProject/MyProject.penelope
        var name = System.IO.Path.GetFileNameWithoutExtension(chosenPath);
        var parentDir = System.IO.Path.GetDirectoryName(chosenPath)!;

        try
        {
            SetCurrentProject(Project.Create(parentDir, name));
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            await ShowError("Could not create project", e.Message);
        }
    }

    public async Task OpenProject(string filePath)
    {
        try
        {
            SetCurrentProject(Project.Load(filePath));
        } catch (Exception e) when (Project.IsLoadError(e))
        {
            RemoveFromRecents(filePath);
            _config.Save();
            await ShowError("Could not open project", e.Message);
        }
    }

    void SetCurrentProject(Project project)
    {
        _currentProject = project;
        Title = $"{project.Name} | Penelope";

        RemoveFromRecents(project.FilePath);
        _config.RecentProjects.Insert(0, project.FilePath);
        if (_config.RecentProjects.Count > MaxRecentProjects)
        {
            _config.RecentProjects.RemoveRange(MaxRecentProjects, _config.RecentProjects.Count - MaxRecentProjects);
        }
        _config.Save();

        NavigateHome();
    }

    Project? TryLoadMostRecentProject()
    {
        var path = _config.RecentProjects.FirstOrDefault();
        if (path is null || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return Project.Load(path);
        }
        catch (Exception e) when (Project.IsLoadError(e))
        {
            return null;
        }
    }

    void RemoveFromRecents(string filePath) => _config.RecentProjects.RemoveAll(p => string.Equals(p, filePath, StringComparison.OrdinalIgnoreCase));

    // Dialogs
    async Task ShowError(string title, string message)
    {
        var ok = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var dialog = new Window
        {
            Title = title,
            Width = 420,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(16),
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                    },
                    ok
                }
            }
        };

        ok.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(this);
    }

    /*
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

    public async Task OpenExistingProject()
    {
        try {
            var startLocation = await StorageProvider.TryGetFolderFromPathAsync(_config.EffectiveProjectsRoot);

            var projectFolder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Penelope Project",
                SuggestedStartLocation = startLocation,
                AllowMultiple = false,
            });

            Console.WriteLine(projectFolder.First().Path.LocalPath);
            _config.ProjectsRoot = projectFolder.First().Path.LocalPath;
            _config.Save();
        } catch (Exception e)
        {
            // TODO: handle exception
        }
    }
    */
}