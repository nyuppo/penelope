#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;

namespace penelope.layout.viewer;

public sealed class ImageContent : FileContent
{
    readonly Image _image;

    List<Bitmap> _frames = new();
    int[] _durations = Array.Empty<int>();
    int _frameIndex;
    readonly DispatcherTimer _animTimer;

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

    public ImageContent()
    {
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

        Content = _viewport;
    }

    public override async Task LoadAsync(string path, CancellationToken ct)
    {
        var r = await Task.Run(() => DecodeFrames(path), ct);

        if (ct.IsCancellationRequested)
        {
            foreach (var f in r.frames)
            {
                f.Dispose();
            }

            return;
        }

        _frames = r.frames;
        _durations = r.durations;
        _frameIndex = 0;

        _image.Source = _frames[0];
        _image.IsVisible = true;

        var imgSize = _frames[0].PixelSize;
        SetMeta($"{imgSize.Width} × {imgSize.Height}");

        if (_frames.Count > 1)
        {
            _animTimer.Interval = TimeSpan.FromMilliseconds(_durations[0]);
            _animTimer.Start();
        }
    }

    public override void Dispose()
    {
        _animTimer.Stop();
        _image.Source = null;
        foreach (var f in _frames)
        {
            f.Dispose();
        }
        _frames.Clear();
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
        // convert cursor from viewport space into image local space
        var p = e.GetPosition(_viewport) - _image.Bounds.Position;
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

    void ClampPan()
    {
        if (_image.Source is null)
        {
            return;
        }
        var vp = _viewport.Bounds.Size;
        var b = _image.Bounds;
        var r = new Rect(b.Size).TransformToAABB(_view).Translate(b.Position);

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