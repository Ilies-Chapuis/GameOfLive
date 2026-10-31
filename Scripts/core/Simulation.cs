// Assets/Scripts/Core/Simulation.cs
// Applique les règles de Conway. Déterministe : pas de Random, pas de Time.

using System;

public class Simulation
{
    public GridModel Grid { get; }
    public IBorderRule BorderRule { get; set; }
    public int Generation { get; private set; }

    public event Action OnGridChanged;

    private bool[] _next;

    public Simulation(GridModel grid, IBorderRule borderRule)
    {
        Grid = grid;
        BorderRule = borderRule;
        _next = new bool[grid.Width * grid.Height];
    }

    public void Step()
    {
        int w = Grid.Width, h = Grid.Height;

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int n = CountNeighbours(x, y);
            bool alive = Grid.Get(x, y);

            // Morte + 3 voisines → naît ; vivante + 2 ou 3 → survit ; sinon meurt.
            _next[y * w + x] = alive ? (n == 2 || n == 3) : (n == 3);
        }

        // On a lu uniquement l'ancienne génération, on bascule d'un coup.
        Grid.SwapBuffer(ref _next);
        Generation++;
        OnGridChanged?.Invoke();
    }

    private int CountNeighbours(int x, int y)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (dx == 0 && dy == 0) continue;
            if (BorderRule.IsAlive(Grid, x + dx, y + dy)) count++;
        }
        return count;
    }

    /// chargement png
    public void NotifyChanged() => OnGridChanged?.Invoke();

    public void ResetGeneration() => Generation = 0;
}