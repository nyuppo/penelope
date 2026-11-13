#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace penelope.layout;

public class FileViewer : UserControl
{
    readonly GlobalConfig _config;
    readonly TextBlock _pathText;

    public string? FilePath { get; private set; }

    static readonly HashSet<string> ValidImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".ico",
    };
    readonly Image _image;

    public FileViewer(GlobalConfig config)
    {
        _config = config;

        _image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            IsVisible = false,
        };

        var headerRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Margin = new Thickness(2),
        };

        headerRow.Children.Add(new TextBlock
        {
            Text = "File Preview",
            FontSize = 12,
        });

        _pathText = new TextBlock
        {
            FontSize = 12,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            TextTrimming = TextTrimming.PrefixCharacterEllipsis,
            Margin = new Thickness(8, 0, 0, 0), // min gap of 8px
            IsVisible = false,
        };
        Grid.SetColumn(_pathText, 1);
        headerRow.Children.Add(_pathText);

        var header = new Border
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = headerRow,
        };
        DockPanel.SetDock(header, Dock.Top);

        Content = new DockPanel { Children = { header, _image } };
    }

    public void SetFile(string? filePath)
    {
        FilePath = filePath;
        _pathText.Text = filePath;
        _pathText.IsVisible = !string.IsNullOrEmpty(filePath);

        // dispose old bitmap
        var old = _image.Source as Bitmap;
        _image.Source = null;
        _image.IsVisible = false;
        old?.Dispose();

        if (filePath != null && ValidImageExtensions.Contains(Path.GetExtension(filePath)))
        {
            try
            {
                _image.Source = new Bitmap(filePath);
                _image.IsVisible = true;
            } catch (Exception)
            {
                // TODO: handle corrupt or unsupported file
            }
        }
    }
}
