#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace penelope.layout;

public class FileBrowser : UserControl
{
    readonly GlobalConfig _config;

    readonly HomeView _owner;
    readonly StackPanel _treeHost = new();
    readonly TextBlock _rootName = new() {
        Classes = { "overline" },
        VerticalAlignment = VerticalAlignment.Center
    };
    readonly TextBlock _rootCount = new() {
        Classes = { "meta" },
        VerticalAlignment = VerticalAlignment.Center
    };
    Border? _selected;

    public FileBrowser(GlobalConfig config, HomeView owner)
    {
        _config = config;
        _owner = owner;

        DockPanel.SetDock(_rootCount, Dock.Right);
        var header = new DockPanel
        {
            Classes = { "tree-label" },
            Children = { _rootCount, _rootName },
        };
        DockPanel.SetDock(header, Dock.Top);
        
        Content = new Border
        {
            Classes = { "tree" },
            Child = new DockPanel { Children = {
                header,
                new ScrollViewer { Content = _treeHost }
            }},
        };

        _config.Changed += RefreshTree;
        RefreshTree();
    }

    public void RefreshTree()
    {
        if (_selected != null)
        {
            _owner.ViewFile(null);
        }
        _selected = null;
        _treeHost.Children.Clear();

        var dir = new DirectoryInfo(_config.EffectiveRoot);
        _rootName.Text = dir.Name.ToUpperInvariant();
        ToolTip.SetTip(_rootName, dir.FullName);

        if (!dir.Exists)
        {
            _rootCount.Text = "";
            return;
        }
        _rootCount.Text = CountEntries(dir)?.ToString() ?? "";
        foreach (var node in BuildNodes(dir, 0))
        {
            _treeHost.Children.Add(node);
        }
    }

    List<Control> BuildNodes(DirectoryInfo dir, int depth)
    {
        var nodes = new List<Control>();
        DirectoryInfo[] dirs;
        FileInfo[] files;

        try
        {
            dirs = dir.GetDirectories();
            files = dir.GetFiles();
        } catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            return nodes;
        }

        foreach (var sub in dirs.OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
        {
            nodes.Add(BuildFolder(sub, depth));
        }

        foreach (var file in files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
        {
            var row = MakeRow(file, depth, IconFor(file), null, isDir: false);
            row.PointerPressed += (_, e) =>
            {
                if (!e.GetCurrentPoint(row).Properties.IsLeftButtonPressed)
                {
                    return;
                }
                Select(row);
                _owner.ViewFile(file.FullName);
            };
            nodes.Add(row);
        }

        return nodes;
    }

    Control BuildFolder(DirectoryInfo dir, int depth)
    {
        var row = MakeRow(dir, depth, Icons.Folder, CountEntries(dir), isDir: true);
        var children = new StackPanel { IsVisible = false };
        var loaded = false;

        row.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(row).Properties.IsLeftButtonPressed)
            {
                return;
            }

            if (!loaded)
            {
                children.Children.AddRange(BuildNodes(dir, depth + 1));
                loaded = true;
            }
            children.IsVisible = !children.IsVisible;
            row.Classes.Set("expanded", children.IsVisible);
        };

        return new StackPanel { Children = { row, children } };
    }

    static Border MakeRow(FileSystemInfo info, int depth, Bitmap? icon, int? count, bool isDir)
    {
        var grid = new Grid {
            ColumnDefinitions = new ColumnDefinitions("12,16,*,Auto"),
            ColumnSpacing = 8
        };

        if (isDir)
        {
            grid.Children.Add(Icons.Glyph(Icons.Chevron, "chevron"));
        }

        var glyph = Icons.Glyph(icon);
        Grid.SetColumn(glyph, 1);
        grid.Children.Add(glyph);

        var name = new TextBlock
        {
            Text = info.Name,
            Classes = { "body", "name" },
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(name, 2);
        grid.Children.Add(name);

        if (count is int c)
        {
            var countText = new TextBlock {
                Text = c.ToString(),
                Classes = { "hand_small", "soft" },
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(countText, 3);
            grid.Children.Add(countText);
        }

        return new Border
        {
            Classes = { "row" },
            Tag = info,
            Padding = new Thickness(8 + depth * 16, 0, 8, 0),
            Child = new Panel { Children = { new Border { Classes = { "highlighter" } }, grid } },
        };
    }

    void Select(Border row)
    {
        _selected?.Classes.Set("selected", false);
        _selected = row;
        row.Classes.Set("selected", true);
    }

    static Bitmap? IconFor(FileInfo f) => FileViewer.ValidImageExtensions.Contains(f.Extension) ? Icons.Image : Icons.Document;

    static int? CountEntries(DirectoryInfo d)
    {
        try
        {
            return d.EnumerateFileSystemInfos().Count();
        } catch (Exception e) when (e is UnauthorizedAccessException or IOException)
        {
            return null;
        }
    }
}