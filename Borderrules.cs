public interface IBorderRule
{
    // Neighbour alive?
    bool IsAlive(GridModel grid, int x, int y);
}

public class DeadBorderRule : IBorderRule
{
    // Outside is dead
    public bool IsAlive(GridModel grid, int x, int y)
    {
        if (!grid.InBounds(x, y))
        {
            return false;
        }
        return grid.Get(x, y);
    }
}

public class WrapBorderRule : IBorderRule
{
    // Wraps edges (torus)
    public bool IsAlive(GridModel grid, int x, int y)
    {
        int wx = ((x % grid.Width) + grid.Width) % grid.Width;
        int wy = ((y % grid.Height) + grid.Height) % grid.Height;
        return grid.Get(wx, wy);
    }
}