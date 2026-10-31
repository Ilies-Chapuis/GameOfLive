// Assets/Scripts/Core/GridModel.cs
// Donnée pure : aucune dépendance à Unity → testable et indépendante de l'affichage.

public class GridModel
{
    public int Width  { get; }
    public int Height { get; }

    private bool[] _cells;

    public GridModel(int width, int height)
    {
        Width  = width;
        Height = height;
        _cells = new bool[width * height];
    }

    private int Index(int x, int y) => y * Width + x;

    public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public bool Get(int x, int y)            => _cells[Index(x, y)];
    public void Set(int x, int y, bool alive) => _cells[Index(x, y)] = alive;
    public void Toggle(int x, int y)          => _cells[Index(x, y)] = !_cells[Index(x, y)];

    public void Clear() => System.Array.Clear(_cells, 0, _cells.Length);

    public int CountAlive()
    {
        int n = 0;
        foreach (bool c in _cells) if (c) n++;
        return n;
    }

    // Utilisé par Simulation pour le double buffer : on échange les tableaux
    // au lieu de recopier → toutes les cellules évoluent "en même temps".
    internal bool[] Raw => _cells;
    internal void SwapBuffer(ref bool[] other)
    {
        (_cells, other) = (other, _cells);
    }
}