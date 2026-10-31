// Assets/Scripts/Core/BorderRules.cs
// Pattern Strategy : le mode de bord est un objet interchangeable,
// les règles de Conway n'ont pas besoin de savoir lequel est utilisé.

public interface IBorderRule
{
    bool IsAlive(GridModel grid, int x, int y);
}

/// Mode Infini
public class DeadBorderRule : IBorderRule
{
    public bool IsAlive(GridModel grid, int x, int y)
        => grid.InBounds(x, y) && grid.Get(x, y);
}

/// Mode Symétrique
public class WrapBorderRule : IBorderRule
{
    public bool IsAlive(GridModel grid, int x, int y)
    {
        int wx = ((x % grid.Width)  + grid.Width)  % grid.Width;   // gère aussi x = -1
        int wy = ((y % grid.Height) + grid.Height) % grid.Height;
        return grid.Get(wx, wy);
    }
}