#nullable enable

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using penelope.layout;

public class HomeView : UserControl
{
    readonly GlobalConfig _config;
    readonly FileBrowser _fileBrowser;
    readonly FileViewer _fileViewer;
    readonly ContentControl _contentColumn = new()
    {
        Margin = new Thickness(8)
    };

    public HomeView(GlobalConfig config)
    {
        _config = config;
        _fileBrowser = new FileBrowser(_config, this);
        _fileViewer = new FileViewer(_config);

        var body = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(250, GridUnitType.Pixel)
                {
                    MinWidth = 232
                },
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(1, GridUnitType.Star)
                {
                    MinWidth = 232
                }
            }
        };

        var browserColumn = _fileBrowser;

        var splitter = new GridSplitter {
            Classes = { "divider" },
            Width = 1,
            ResizeDirection = GridResizeDirection.Columns
        };
        Grid.SetColumn(splitter, 1);

        Grid.SetColumn(_contentColumn, 2);

        body.Children.Add(browserColumn);
        body.Children.Add(splitter);
        body.Children.Add(_contentColumn);

        Content = body;
    }

    public enum ContentType
    {
        None,
        FilePreview,
    }
    private ContentType currentContent = ContentType.None;
    public ContentType CurrentContent => currentContent;

    public bool RequestContentType(ContentType requestedType)
    {
        currentContent = requestedType;
        return true;
    }

    private void RenderContent()
    {
        _contentColumn.Content = currentContent switch
        {
            ContentType.FilePreview => _fileViewer,
            _ => null,
        };
    }

    public void ViewFile(string? filePath)
    {
        if (filePath == null)
        {
            currentContent = ContentType.None;
        } else if (RequestContentType(ContentType.FilePreview))
        {
            _fileViewer.SetFile(filePath);
        }

        RenderContent();
    }
}