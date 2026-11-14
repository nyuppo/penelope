#nullable enable

using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace penelope;

public static class Icons
{
    public static readonly Bitmap? Chevron = Load("chevron");
    public static readonly Bitmap? Folder = Load("folder");
    public static readonly Bitmap? Document = Load("document");
    public static readonly Bitmap? Image = Load("image");

    public static readonly Bitmap? Link = Load("link");
    public static readonly Bitmap? LinkExternal = Load("link_external");

    static Bitmap? Load(string name)
    {
        var uri = new Uri($"avares://penelope/Assets/Icons/{name}.png");
        return AssetLoader.Exists(uri) ? new Bitmap(AssetLoader.Open(uri)) : null;
    }

    public static Border Glyph(Bitmap? img, params string[] classes)
    {
        var b = new Border();
        b.Classes.Add("glyph");
        b.Classes.AddRange(classes);

        if (img is null)
        {
            b.Classes.Add("placeholder");
        }
        else
        {
            b.OpacityMask = new ImageBrush(img) { Stretch = Stretch.Uniform };
        }

        RenderOptions.SetBitmapInterpolationMode(b, BitmapInterpolationMode.HighQuality);
        return b;
    }
}