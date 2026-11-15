using System;

public class Simulation
{
    public GridModel Grid { get; private set; }
    public IBorderRule BorderRule { get; set; }
    public int Generation { get; private set; }

    public event Action OnGridChanged;

    private bool[] _next;

    // Setup simulation
    public Simulation(GridModel grid, IBorderRule borderRule)
    {
        Grid = grid;
        BorderRule = borderRule;
        _next = new bool[grid.Width * grid.Height];
    }

    // Next generation
    public void Step()
    {
        int w = Grid.Width;
        int h = Grid.Height;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int n = CountNeighbours(x, y);
                bool alive = Grid.Get(x, y);
                bool nextAlive;

                if (alive)
                {
                    nextAlive = (n == 2 || n == 3);
                }
                else
                {
                    nextAlive = (n == 3);
                }

                _next[y * w + x] = nextAlive;
            }
        }

        Grid.SwapBuffer(ref _next);
        Generation++;
        OnGridChanged?.Invoke();
    }

    // Count Moore neighbours
    private int CountNeighbours(int x, int y)
    {
        int count = 0;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }
                if (BorderRule.IsAlive(Grid, x + dx, y + dy))
                {
                    count++;
                }
            }
        }
        return count;
    }

    // Force refresh
    public void NotifyChanged()
    {
        OnGridChanged?.Invoke();
    }

    // Reset counter
    public void ResetGeneration()
    {
        Generation = 0;
    }
}