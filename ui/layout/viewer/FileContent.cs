using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace penelope.layout.viewer;

public abstract class FileContent : UserControl, IDisposable
{
    public string Meta { get; private set; } = "";
    public event Action? MetaChanged;
    protected void SetMeta(string meta)
    {
        Meta = meta;
        MetaChanged?.Invoke();
    }

    public virtual IReadOnlyList<Control> HeaderActions => Array.Empty<Control>();

    public abstract Task LoadAsync(string path, CancellationToken ct);

    public virtual void Dispose() {}
}