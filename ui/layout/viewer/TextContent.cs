#nullable enable

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace penelope.layout.viewer;

public sealed class TextDocument
{
    const long MaxBytes = 2 * 1024 * 1024;

    public required string FilePath { get; init; }
    public required string Text { get; set; }
    public required Encoding Encoding { get; init; }
    public required string NewLine { get; init; }

    public static async Task<TextDocument> LoadAsync(string path, CancellationToken ct)
    {
        if (new FileInfo(path).Length > MaxBytes)
        {
            throw new InvalidDataException("File is too large to preview.");
        }

        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = await reader.ReadToEndAsync(ct);

        return new TextDocument
        {
            FilePath = path,
            Text = text,
            Encoding = reader.CurrentEncoding,
            NewLine = text.Contains("\r\n") ? "\r\n" : "\n",
        };
    }
}

public sealed class TextContent : FileContent
{
    const double RuleSpacing = 28;

    readonly TextBox _text;
    TextDocument? _doc;

    public TextContent()
    {
        _text = new TextBox
        {
            Classes = { "page" },
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = RuleSpacing,
            Padding = new Thickness(72, 12, 24, 28),
        };

        Content = new ScrollViewer
        {
            Content = new Border
            {
                Classes = { "sheet" },
                Margin = new Thickness(24),
                Child = new Panel
                {
                    Children = { new RuledLines(), _text },
                }
            }
        };
    }

    public override async Task LoadAsync(string path, CancellationToken ct)
    {
        var doc = await TextDocument.LoadAsync(path, ct);
        if (ct.IsCancellationRequested)
        {
            return;
        }

        _doc = doc;
        _text.Text = doc.Text;

        var lines = doc.Text.Split('\n').Length;
        SetMeta($"{lines} lines");
    }
}