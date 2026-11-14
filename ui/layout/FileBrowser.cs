#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace penelope.layout;

sealed class Node
{
    public required FileSystemInfo Info;
    public required Border Row;
    public required Control View { get; init; }
    public required int Depth { get; init; }
    public Node? Parent;
    public StackPanel? ChildHost;
    public List<Node> Children { get; } = new();
    public bool Loaded { get; set; }

    public bool IsDir => ChildHost != null;
    public bool IsExpanded => ChildHost?.IsVisible == true;
}

public class FileBrowser : UserControl
{
    readonly Project _project;

    readonly HomeView _owner;
    readonly StackPanel _treeHost = new();
    readonly TextBlock _rootName = new() {
        Classes = { "overline" },
        VerticalAlignment = VerticalAlignment.Center
    };
    readonly List<Node> _roots = new();
    Node? _selected;
    Node? _active;

    public FileBrowser(Project project, HomeView owner)
    {
        _project = project;
        _owner = owner;

        Focusable = true;
        FocusAdorner = null;
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

        var header = new DockPanel
        {
            Classes = { "tree-label" },
            Children = { _rootName },
        };
        DockPanel.SetDock(header, Dock.Top);
        
        Content = new Border
        {
            Classes = { "tree" },
            Child = new DockPanel { Children = {
                header,
                new ScrollViewer {
                    Content = _treeHost,
                    Background = Brushes.Transparent,
                    ContextMenu = new ContextMenu
                    {
                        Items =
                        {
                            Item("New folder...", () => CreateFolder(_project.RootPath, "New Folder")),
                            Item("New file...", () => {}),
                            new Separator(),
                            Item("Refresh", RefreshTree),
                        }
                    }
                }
            }},
        };

        RefreshTree();
    }

    public void RefreshTree()
    {
        if (_active != null)
        {
            _owner.ViewFile(null);
            SetActive(null);
        }
        _selected = null;
        _roots.Clear();
        _treeHost.Children.Clear();

        var dir = new DirectoryInfo(_project.RootPath);
        _rootName.Text = _project.Name.ToUpperInvariant();
        ToolTip.SetTip(_rootName, dir.FullName);

        if (!dir.Exists)
        {
            return;
        }
        foreach (var node in BuildNodes(dir, 0, null))
        {
            _roots.Add(node);
            _treeHost.Children.Add(node.View);
        }
    }

    List<Node> BuildNodes(DirectoryInfo dir, int depth, Node? parent)
    {
        var nodes = new List<Node>();
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
            nodes.Add(BuildFolder(sub, depth, parent));
        }

        foreach (var file in files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
        {
            var row = MakeRow(file, depth, IconFor(file), isDir: false);
            var node = new Node
            {
                Info = file,
                Row = row,
                View = row,
                Depth = depth,
                Parent = parent
            };
            row.Tag = node;
            row.ContextMenu = FileMenu(file, row);
            HookPointer(node);
            nodes.Add(node);
        }

        return nodes;
    }

    Node BuildFolder(DirectoryInfo dir, int depth, Node? parent)
    {
        var row = MakeRow(dir, depth, Icons.Folder, isDir: true);
        var childHost = new StackPanel { IsVisible = false };
        var node = new Node
        {
            Info = dir,
            Row = row,
            View = new StackPanel { Children = { row, childHost } },
            Depth = depth,
            Parent = parent,
            ChildHost = childHost,
        };
        row.Tag = node;
        row.ContextMenu = FolderMenu(dir, row);
        HookPointer(node);
        return node;
    }

    void HookPointer(Node node)
    {
        node.Row.PointerPressed += (_, e) =>
        {
            if (!e.GetCurrentPoint(node.Row).Properties.IsLeftButtonPressed)
            {
                return;
            }

            Focus();
            Select(node);
            Activate(node);
        };
    }

    void Activate(Node node)
    {
        if (node.IsDir)
        {
            if (node.IsExpanded)
            {
                Collapse(node);
            } else
            {
                Expand(node);
            }
        } else
        {
            SetActive(node);
            _owner.ViewFile(node.Info.FullName);
        }
    }

    void SetActive(Node? node)
    {
        _active?.Row.Classes.Set("active", false);
        _active = node;
        node?.Row.Classes.Set("active", true);
    }

    void Expand(Node node)
    {
        if (!node.IsDir || node.IsExpanded)
        {
            return;
        }

        if (!node.Loaded)
        {
            foreach (var child in BuildNodes((DirectoryInfo)node.Info, node.Depth + 1, node))
            {
                node.Children.Add(child);
                node.ChildHost!.Children.Add(child.View);
            }
            node.Loaded = true;
        }

        node.ChildHost!.IsVisible = true;
        node.Row.Classes.Set("expanded", true);
    }

    void Collapse(Node node)
    {
        if (!node.IsExpanded)
        {
            return;
        }

        node.ChildHost!.IsVisible = false;
        node.Row.Classes.Set("expanded", false);

        // don't leave the cursor on a row that just got hidden
        if (_selected != null && IsDescendant(_selected, node))
        {
            Select(node);
        }
    }

    static bool IsDescendant(Node node, Node ancestor)
    {
        for (var p = node.Parent; p != null; p = p.Parent)
        {
            if (p == ancestor)
            {
                return true;
            }
        }
        return false;
    }

    void Select(Node node)
    {
        _selected?.Row.Classes.Set("selected", false);
        _selected = node;
        node.Row.Classes.Set("selected", true);
    }

    static IEnumerable<Node> VisibleNodes(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;

            if (node.IsExpanded)
            {
                foreach (var child in VisibleNodes(node.Children))
                {
                    yield return child;
                }
            }
        }
    }

    void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var visible = VisibleNodes(_roots).ToList();
        if (visible.Count == 0)
        {
            return;
        }

        var i = _selected == null ? -1 : visible.IndexOf(_selected);
        var current = i >= 0 ? visible[i] : null;

        switch (e.Key) {
            case Key.Down:
                MoveTo(visible[Math.Min(i + 1, visible.Count - 1)]);
                break;
            case Key.Up:
                MoveTo(i < 0 ? visible[0] : visible[Math.Max(i - 1, 0)]);
                break;
            case Key.Home:
                MoveTo(visible[0]);
                break;
            case Key.End:
                MoveTo(visible[^1]);
                break;
            case Key.Right when current is { IsDir: true }:
                if (!current.IsExpanded) {
                    Expand(current);
                }
                else if (current.Children.Count > 0) {
                    MoveTo(current.Children[0]);
                }
                break;
            case Key.Left when current != null:
                if (current.IsExpanded)
                {
                    Collapse(current);
                }
                else if (current.Parent != null)
                {
                    MoveTo(current.Parent);
                }
                break;
            case Key.Enter or Key.Space when current != null:
                Activate(current);
                break;
            default:
                return;
        }
    }

    void MoveTo(Node node)
    {
        Select(node);
        node.Row.BringIntoView();
    }

    static Border MakeRow(FileSystemInfo info, int depth, Bitmap? icon, bool isDir)
    {
        var grid = new Grid {
            ColumnDefinitions = new ColumnDefinitions("12,16,*"),
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

        return new Border
        {
            Classes = { "row" },
            Padding = new Thickness(8 + depth * 16, 0, 8, 0),
            Child = new Panel { Children = { new Border { Classes = { "highlighter" } }, grid } },
        };
    }

    static Bitmap? IconFor(FileInfo f) => FileViewer.ValidImageExtensions.Contains(f.Extension) ? Icons.Image : Icons.Document;

    ContextMenu FileMenu(FileInfo file, Border row) => new()
    {
        Items =
        {
            Item("Show in Explorer", () => ShowInFileManager(file.FullName)),
        }
    };

    ContextMenu FolderMenu(DirectoryInfo dir, Border row) => new()
    {
        Items =
        {
            Item("Add folder...", () => CreateFolder(dir.FullName, "New Folder")),
            Item("Add file...", () => {}),
            new Separator(),
            Item("Show in Explorer", () => ShowInFileManager(dir.FullName)),
        }
    };

    static MenuItem Item(string header, Action onClick)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => onClick();
        return item;
    }

    static void ShowInFileManager(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", ["-R", path]);
            }
            else
            {
                Process.Start("xdg-open", [Path.GetDirectoryName(path) ?? path]);
            }
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // no file manager available
        }
    }

    void CreateFolder(string parent, string baseName)
    {
        var path = UniquePath(parent, baseName);

        try
        {
            Directory.CreateDirectory(path);
        } catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // TODO: let user know
            return;
        }

        RefreshTree();
    }

    static string UniquePath(string parent, string baseName)
    {
        var path = Path.Combine(parent, baseName);

        for (int i = 2; Directory.Exists(path) || File.Exists(path); i++)
        {
            path = Path.Combine(parent, $"{baseName} ({i})");
        }

        return path;
    }
}