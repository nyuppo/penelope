#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

    readonly Border _viewport;
    Matrix _view = Matrix.Identity;
    bool _panning;
    Point _lastPan;
    const double MinZoom = 0.1;
    const double MaxZoom = 50;
    const double ZoomStep = 1.15;
    const double ConstantInterpolationZoomThreshold = 1.5;
    const int MinFrameDelayMs = 10;
    const int DefaultFrameDelayMs = 100;
    static readonly Vector StandardDpi = new(96, 96);
    const double MinVisiblePixels = 50;

    public FileViewer(GlobalConfig config)
    {
        _config = config;
        _image = new Image
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            IsVisible = false,
            RenderTransformOrigin = RelativePoint.TopLeft,
        };

        _animTimer = new DispatcherTimer();
        _animTimer.Tick += OnAnimTick;

        _viewport = new Border
        {
            ClipToBounds = true,
            Background = Brushes.Transparent,
            Child = _image,
        };
        _viewport.PointerWheelChanged += OnWheel;
        _viewport.PointerPressed += OnPointerPressed;
        _viewport.PointerMoved += OnPointerMoved;
        _viewport.PointerReleased += OnPointerReleased;
        _viewport.PointerCaptureLost += (_, _) => {
            _panning = false;
            _viewport.Cursor = null;
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

        Content = new DockPanel { Children = { header, _viewport } };
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
        _view = Matrix.Identity;
        _image.RenderTransform = null;

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
            }

            return;
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
        using var codec = SKCodec.Create(path) ?? throw new InvalidDataException(path);
        
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
                        StandardDpi,
                        bmp.RowBytes
                    )
                );
                durations[i] = infos[i].Duration <= MinFrameDelayMs ? DefaultFrameDelayMs : infos[i].Duration;
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

    void ApplyView()
    {
        ClampPan();
        _image.RenderTransform = new MatrixTransform(_view);
        RenderOptions.SetBitmapInterpolationMode(_image, _view.M11 > ConstantInterpolationZoomThreshold ? BitmapInterpolationMode.None : BitmapInterpolationMode.HighQuality);
    }

    void OnWheel(object? s, PointerWheelEventArgs e)
    {
        var p = e.GetPosition(_viewport);
        var f = Math.Clamp(_view.M11 * Math.Pow(ZoomStep, e.Delta.Y), MinZoom, MaxZoom) / _view.M11;
        _view = _view * Matrix.CreateTranslation(-p.X, -p.Y) * Matrix.CreateScale(f, f) * Matrix.CreateTranslation(p.X, p.Y);
        ApplyView();
        e.Handled = true;
    }

    void OnPointerPressed(object? s, PointerPressedEventArgs e)
    {
        if (e.ClickCount == 2) {
            _view = Matrix.Identity;
            ApplyView();
            return;
        }

        if (!e.GetCurrentPoint(_viewport).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _panning = true;
        _lastPan = e.GetPosition(_viewport);
        e.Pointer.Capture(_viewport);
        _viewport.Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    void OnPointerMoved(object? s, PointerEventArgs e)
    {
        if (!_panning)
        {
            return;
        }

        var pos = e.GetPosition(_viewport);
        var d = pos - _lastPan;
        _lastPan = pos;
        _view *= Matrix.CreateTranslation(d.X, d.Y);

        ApplyView();
    }

    void OnPointerReleased(object? s, PointerReleasedEventArgs e)
    {
        if (_panning)
        {
            e.Pointer.Capture(null);
        }
    }

    double FitScale()
    {
        if (_image.Source is not { } src) return 1;
        var vp = _viewport.Bounds.Size;
        var fit = Math.Min(vp.Width / src.Size.Width, vp.Height / src.Size.Height);
        return _image.StretchDirection == StretchDirection.DownOnly ? Math.Min(fit, 1) : fit;
    }

    void ClampPan()
    {
        if (_image.Source is not { } src)
        {
            return;
        }
        var vp = _viewport.Bounds.Size;
        var size = src.Size * FitScale();
        var r = new Rect((vp.Width - size.Width) / 2, (vp.Height - size.Height) / 2, size.Width, size.Height).TransformToAABB(_view);

        double dx = 0, dy = 0;
        if (r.Right < MinVisiblePixels)
        {
            dx = MinVisiblePixels - r.Right;
        }
        else if (r.Left > vp.Width - MinVisiblePixels)
        {
            dx = vp.Width - MinVisiblePixels - r.Left;
        }

        if (r.Bottom < MinVisiblePixels)
        {
            dy = MinVisiblePixels - r.Bottom;
        }
        else if (r.Top > vp.Height - MinVisiblePixels)
        {
            dy = vp.Height - MinVisiblePixels - r.Top;
        }

        _view *= Matrix.CreateTranslation(dx, dy);
    }
}
