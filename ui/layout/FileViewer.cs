#nullable enable

using System;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using penelope.layout.viewer;

namespace penelope.layout;

public class FileViewer : UserControl
{
    readonly GlobalConfig _config;
    readonly Project _project;

    public string? FilePath { get; private set; }

    readonly TextBlock _dirText;
    readonly TextBlock _nameText;
    readonly TextBlock _metaText;
    readonly Border _header;

    readonly ContentControl _host = new();
    FileContent? _content;
    CancellationTokenSource? _loadCts;

    public FileViewer(GlobalConfig config, Project project)
    {
        _config = config;
        _project = project;

        _dirText = new TextBlock
        {
            Classes = { "caption" },
            TextTrimming = TextTrimming.PrefixCharacterEllipsis,
            [!TextBlock.FontSizeProperty] = new DynamicResourceExtension("FontSize_Overline"),
        };
        _nameText = new TextBlock
        {
            Classes = { "title" },
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, -4, 0, 0),
        };
        var titleStack = new StackPanel
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
            Spacing = 0,
            Children = { _dirText, _nameText },
        };

        _metaText = new TextBlock
        {
            Classes = { "mono" },
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0),
        };
        Grid.SetColumn(_metaText, 1);

        var openInExplorerAction = new Button
        {
            Classes = { "icon" },
            Content = Icons.Glyph(Icons.LinkExternal),
            [ToolTip.TipProperty] = "Show in Explorer",
        };
        openInExplorerAction.Click += (_, _) =>
        {
            if (FilePath != null)
            {
                FileBrowser.ShowInFileManager(FilePath);
            }
        };

        var actions = new Border
        {
            Classes = { "header-actions" },
            Child = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 2,
                Children =
                {
                    openInExplorerAction
                }
            }
        };
        Grid.SetColumn(actions, 2);

        _header = new Border
        {
            Classes = { "viewer-header" },
            IsVisible = false,
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
                Children = { titleStack, _metaText, actions },
            },
        };
        DockPanel.SetDock(_header, Dock.Top);

        Content = new DockPanel { Children = { _header, _host } };
    }

    public async void SetFile(string? filePath)
    {
        FilePath = filePath;

        _header.IsVisible = filePath != null;
        if (filePath != null)
        {
            _nameText.Text = Path.GetFileName(filePath);

            var dir = Path.GetDirectoryName(filePath) ?? "";
            var parts = Path.GetRelativePath(_project.RootPath, dir).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            _dirText.Inlines = new InlineCollection();
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    _dirText.Inlines.Add(new Run(" / ")
                    {
                        Foreground = this.FindResource("Colour_Ink_Faint") as IBrush
                    });
                }
                _dirText.Inlines.Add(new Run(parts[i].ToUpperInvariant()));
            }
        }

        _loadCts?.Cancel();
        var cts = _loadCts = new CancellationTokenSource();

        _content?.Dispose();
        _content = filePath != null && ImageContent.ValidImageExtensions.Contains(Path.GetExtension(filePath))
            ? new ImageContent()
            : null;
        _host.Content = _content;
        _metaText.Text = "";

        if (_content == null)
        {
            return;
        }

        var content = _content;
        content.MetaChanged += () =>
        {
            if (content == _content)
            {
                _metaText.Text = content.Meta;
            }
        };

        try
        {
            await content.LoadAsync(filePath!, cts.Token);
        }
        catch (Exception)
        {
            // TODO: handle corrupt or unsupported file
        }
    }
}
