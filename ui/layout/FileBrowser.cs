using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace penelope.layout;

public class FileBrowser : UserControl
{
    readonly GlobalConfig _config;
    readonly TreeView _fileTree = new();

    public FileBrowser(GlobalConfig config, HomeView owner)
    {
        _config = config;

        var header = new TextBlock
        {
            Text = _config.EffectiveRoot,
            FontSize = 12,
            Margin = new Thickness(8),
        };
        _config.Changed += () => header.Text = _config.EffectiveRoot;
        DockPanel.SetDock(header, Dock.Top);

        Content = new DockPanel { Children = { header, _fileTree } };

        _fileTree.SelectionChanged += (sender, args) =>
        {
            if (args.AddedItems.Count == 0)
            {
                owner.RequestContentType(HomeView.ContentType.None);
                return;
            }

            foreach (var item in args.AddedItems)
            {
                if (item is TreeViewItem tvi)
                {
                    var info = tvi.Tag as FileSystemInfo;

                    if (info is FileInfo)
                    {
                        Console.WriteLine("File | " + info.FullName);
                        owner.ViewFile(info.FullName);
                    } else if (info is DirectoryInfo)
                    {
                        Console.WriteLine("Folder | " + info.FullName);
                    } else
                    {
                        Console.WriteLine("Unknown | " + info.FullName);
                    }
                }
            }
        };

        _config.Changed += RefreshTree;
        RefreshTree();
    }

    public void RefreshTree()
    {
        var root = _config.EffectiveRoot;
        _fileTree.ItemsSource = Directory.Exists(root) ? BuildNodes(new DirectoryInfo(root)) : null;
    }

    static List<TreeViewItem> BuildNodes(DirectoryInfo dir)
    {
        var nodes = new List<TreeViewItem>();

        foreach (var sub in dir.EnumerateDirectories())
        {
            var folderItem = new TreeViewItem
            {
                Header = sub.Name,
                Tag = sub,
                ItemsSource = BuildNodes(sub),
                Background = Brushes.AliceBlue
            };
            folderItem.Classes.Add("tree_item_folder");

            nodes.Add(folderItem);
        }

        foreach (var file in dir.EnumerateFiles())
        {
            var fileItem = new TreeViewItem
            {
                Header = file.Name,
                Tag = file,
                Background = Brushes.LightBlue
            };
            fileItem.Classes.Add("tree_item_file");

            nodes.Add(fileItem);
        }

        return nodes;
    }
}