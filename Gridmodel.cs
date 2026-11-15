public class GridModel
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private bool[] _cells;

    // Empty grid
    public GridModel(int width, int height)
    {
        Width = width;
        Height = height;
        _cells = new bool[width * height];
    }

    // 2D to 1D
    private int Index(int x, int y)
    {
        return y * Width + x;
    }

    // Inside grid?
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    // Read cell
    public bool Get(int x, int y)
    {
        return _cells[Index(x, y)];
    }

    // Write cell
    public void Set(int x, int y, bool alive)
    {
        _cells[Index(x, y)] = alive;
    }

    // Flip cell
    public void Toggle(int x, int y)
    {
        int i = Index(x, y);
        _cells[i] = !_cells[i];
    }

    // Kill all
    public void Clear()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i] = false;
        }
    }

    // Living cells count
    public int CountAlive()
    {
        int count = 0;
        for (int i = 0; i < _cells.Length; i++)
        {
            if (_cells[i])
            {
                count++;
            }
        }
        return count;
    }

    // Double buffer swap
    public void SwapBuffer(ref bool[] other)
    {
        bool[] temp = _cells;
        _cells = other;
        other = temp;
    }
}