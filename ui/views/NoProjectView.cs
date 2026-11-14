#nullable enable

using Avalonia.Controls;
using penelope;

public class NoProjectView : UserControl
{
    public NoProjectView(MainWindow owner)
    {
        var newProject = new Button { Content = "New Project..." };
        newProject.Click += async (_, _) => await owner.CreateNewProject();

        var openProject = new Button { Content = "Open Project..." };
        openProject.Click += async (_, _) => await owner.OpenExistingProject();

        Content = new StackPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Spacing = 12,
            Children =
            {
                new TextBlock
                {
                    Text = "No project open",
                    FontSize = 20,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 8,
                    Children = { newProject, openProject, },
                }
            }
        };
    }
}