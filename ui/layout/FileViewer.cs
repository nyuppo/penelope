#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;

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

    List<Bitmap> _frames = new();
    int[] _durations = Array.Empty<int>();
    int _frameIndex;
    readonly DispatcherTimer _animTimer;
    int _loadId;

    public FileViewer(GlobalConfig config)
    {
        _config = config;
        _image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            IsVisible = false,
        };

        _animTimer = new DispatcherTimer();
        _animTimer.Tick += OnAnimTick;

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

    public async void SetFile(string? filePath)
    {
        FilePath = filePath;
        _pathText.Text = filePath;
        _pathText.IsVisible = !string.IsNullOrEmpty(filePath);

        var id = ++_loadId;

        // dispose old
        _animTimer.Stop();
        _image.Source = null;
        _image.IsVisible = false;
        foreach (var f in _frames)
        {
            f.Dispose();
        }
        _frames.Clear();

        if (filePath == null || !ValidImageExtensions.Contains(Path.GetExtension(filePath)))
        {
            return;
        }

        (List<Bitmap> frames, int[] durations) r;
        try
        {
            r = await Task.Run(() => DecodeFrames(filePath));
        } catch (Exception)
        {
            // TODO: handle corrupt or unsupported file
            return;
        }

        // handle stale load
        if (id != _loadId)
        {
            foreach (var f in r.frames)
            {
                f.Dispose();
                return;
            }
        }

        (_frames, _durations, _frameIndex) = (r.frames, r.durations, 0);
        _image.Source = _frames[0];
        _image.IsVisible = true;
        if (_frames.Count > 1)
        {
            _animTimer.Interval = TimeSpan.FromMilliseconds(_durations[0]);
            _animTimer.Start();
        }
    }

    void OnAnimTick(object? s, EventArgs e)
    {
        _frameIndex = (_frameIndex + 1) % _frames.Count;
        _image.Source = _frames[_frameIndex];
        _animTimer.Interval = TimeSpan.FromMilliseconds(_durations[_frameIndex]);
    }

    static (List<Bitmap> frames, int[] durations) DecodeFrames(string path)
    {
        using var codec = SKCodec.Create(path);
        
        var infos = codec.FrameInfo;
        if (infos.Length <= 1) return (new() { new Bitmap(path) }, new[] { 0 });

        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var sk = new List<SKBitmap>();
        var frames = new List<Bitmap>();
        var durations = new int[infos.Length];

        try
        {
            for (int i = 0; i < infos.Length; i++)
            {
                int req = infos[i].RequiredFrame;
                var bmp = req >= 0 ? sk[req].Copy() : new SKBitmap(info);
                if (req < 0) bmp.Erase(SKColors.Transparent);

                sk.Add(bmp);
                codec.GetPixels(info, bmp.GetPixels(), new SKCodecOptions(i, req));
                frames.Add(
                    new Bitmap(
                        PixelFormat.Bgra8888,
                        AlphaFormat.Premul,
                        bmp.GetPixels(),
                        new PixelSize(bmp.Width, bmp.Height),
                        new Vector(96, 96),
                        bmp.RowBytes
                    )
                );
                durations[i] = infos[i].Duration <= 10 ? 100 : infos[i].Duration;
            }
        }
        finally {
            foreach (var b in sk)
            {
                b.Dispose();
            }
        }

        return (frames, durations);
    }
}
