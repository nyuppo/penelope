using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

public class SettingsView : UserControl
{
    readonly GlobalConfig _config;

    public SettingsView(GlobalConfig config, Action onDone)
    {
        _config = config;

        var rootBox = new TextBox
        {
            Text = config.AppRoot ?? "",
            PlaceholderText = AppPaths.DefaultAppRootPath,
        };

        var save = new Button { Content = "Save" };
        save.Click += (_, _) =>
        {
            config.AppRoot = string.IsNullOrWhiteSpace(rootBox.Text) ? null : rootBox.Text.Trim();
            config.Save();
            onDone();
        };

        var cancel = new Button { Content = "Cancel" };
        cancel.Click += (_, _) => onDone();

        Content = new StackPanel
        {
            Margin = new Thickness(16),
            Spacing = 8,
            MaxWidth = 600,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                new TextBlock { Text = "Settings", FontSize = 20 },
                new TextBlock { Text = "App root folder" },
                rootBox,
                new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { save, cancel } },
            }
        };
    }
}