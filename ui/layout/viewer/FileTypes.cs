#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;

namespace penelope.layout.viewer;

public sealed record FileType(string Name, Bitmap? Icon, Func<FileContent> Create, params string[] Extensions);

public static class FileTypes
{
    static readonly FileType[] All =
    {
        new("Image", Icons.Image, () => new ImageContent(), ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp", ".ico"),

    };

    static readonly Dictionary<string, FileType> ByExtension = Build();

    static Dictionary<string, FileType> Build()
    {
        var map = new Dictionary<string, FileType>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in All)
        {
            foreach (var ext in type.Extensions)
            {
                map[ext] = type;
            }
        }
        return map;
    }

    public static FileType? For(string path) => ByExtension.GetValueOrDefault(Path.GetExtension(path));
}