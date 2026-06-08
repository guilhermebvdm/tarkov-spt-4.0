namespace CustomClasses;

/// <summary>
///     Packer first-fit de uma única grade (rig/mochila/stash). Marca células ocupadas e acha a
///     primeira posição livre para um item w×h, tentando rotação. Ver docs/technical/inventario-itens-spt4.md §4/§5.
/// </summary>
internal sealed class GridPacker(int width, int height)
{
    private readonly bool[,] _cells = new bool[Math.Max(0, height), Math.Max(0, width)];

    public int Width { get; } = Math.Max(0, width);
    public int Height { get; } = Math.Max(0, height);

    /// <summary>Tenta posicionar um item w×h (sem rotação, depois rotacionado). Marca e retorna (x,y,rotacionado) ou null.</summary>
    public (int X, int Y, bool Rotated)? Place(int w, int h)
    {
        if (w <= 0 || h <= 0)
        {
            return null;
        }

        foreach (var rotated in new[] { false, true })
        {
            var iw = rotated ? h : w;
            var ih = rotated ? w : h;
            if (iw > Width || ih > Height)
            {
                continue;
            }

            for (var y = 0; y <= Height - ih; y++)
            {
                for (var x = 0; x <= Width - iw; x++)
                {
                    if (Fits(x, y, iw, ih))
                    {
                        Mark(x, y, iw, ih);
                        return (x, y, rotated);
                    }
                }
            }
        }

        return null;
    }

    private bool Fits(int x, int y, int w, int h)
    {
        for (var j = y; j < y + h; j++)
        {
            for (var i = x; i < x + w; i++)
            {
                if (_cells[j, i])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void Mark(int x, int y, int w, int h)
    {
        for (var j = y; j < y + h; j++)
        {
            for (var i = x; i < x + w; i++)
            {
                _cells[j, i] = true;
            }
        }
    }
}
