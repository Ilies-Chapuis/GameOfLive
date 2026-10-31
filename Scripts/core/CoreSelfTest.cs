// Assets/Scripts/Tests/CoreSelfTest.cs
// À poser sur un GameObject vide dans une scène "Tests", puis lancer Play.


using UnityEngine;

public class CoreSelfTest : MonoBehaviour
{
    private void Start()
    {
        Check("Blinker oscille (période 2)",                TestBlinker());
        Check("Bloc reste stable",                          TestBlock());
        Check("Planeur fait le tour en mode Symétrique",    TestGliderWrap());
        Check("Planeur ne revient pas en mode Infini",      TestGliderDead());
        Check("Déterminisme (même départ = même résultat)", TestDeterminism());
        Check("Aller-retour PNG sur grille non carrée",     TestPngRoundTrip());
    }

    private static void Check(string name, bool ok)
    {
        if (ok) Debug.Log($"cool {name}");
        else    Debug.LogError($"ça marche pas {name}");
    }

    //  Tests

    private static bool TestBlinker()
    {
        var g = Make(5, 5, (1, 2), (2, 2), (3, 2)); // ligne horizontale
        var start = Copy(g);
        var sim = new Simulation(g, new DeadBorderRule());

        sim.Step();
        bool vertical = g.Get(2, 1) && g.Get(2, 2) && g.Get(2, 3) && g.CountAlive() == 3;
        sim.Step();
        return vertical && Same(g, start);
    }

    private static bool TestBlock()
    {
        var g = Make(4, 4, (1, 1), (2, 1), (1, 2), (2, 2));
        var start = Copy(g);
        var sim = new Simulation(g, new DeadBorderRule());
        for (int i = 0; i < 5; i++) sim.Step();
        return Same(g, start);
    }

    private static GridModel Glider8x8() => Make(8, 8, (1, 0), (2, 1), (0, 2), (1, 2), (2, 2));

    private static bool TestGliderWrap()
    {
        var g = Glider8x8();
        var start = Copy(g);
        var sim = new Simulation(g, new WrapBorderRule());
        for (int i = 0; i < 32; i++) sim.Step();
        return Same(g, start);
    }

    private static bool TestGliderDead()
    {
        var g = Glider8x8();
        var start = Copy(g);
        var sim = new Simulation(g, new DeadBorderRule());
        for (int i = 0; i < 32; i++) sim.Step();
        return !Same(g, start);
    }

    private static bool TestDeterminism()
    {
        var a = RandomGrid(40, 25, 42);
        var b = RandomGrid(40, 25, 42);
        var simA = new Simulation(a, new WrapBorderRule());
        var simB = new Simulation(b, new WrapBorderRule());
        for (int i = 0; i < 100; i++) { simA.Step(); simB.Step(); }
        return Same(a, b);
    }

    private static bool TestPngRoundTrip()
    {
        var g = RandomGrid(37, 21, 7);
        var back = PngGridSerializer.Decode(PngGridSerializer.Encode(g));
        return Same(g, back);
    }

    // 

    private static GridModel Make(int w, int h, params (int x, int y)[] cells)
    {
        var g = new GridModel(w, h);
        foreach (var c in cells) g.Set(c.x, c.y, true);
        return g;
    }

    private static GridModel RandomGrid(int w, int h, int seed)
    {
        var rng = new System.Random(seed); // seed fixe → toujours la même grille
        var g = new GridModel(w, h);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            g.Set(x, y, rng.NextDouble() < 0.3);
        return g;
    }

    private static GridModel Copy(GridModel src)
    {
        var g = new GridModel(src.Width, src.Height);
        for (int y = 0; y < src.Height; y++)
        for (int x = 0; x < src.Width; x++)
            g.Set(x, y, src.Get(x, y));
        return g;
    }

    private static bool Same(GridModel a, GridModel b)
    {
        if (a.Width != b.Width || a.Height != b.Height) return false;
        for (int y = 0; y < a.Height; y++)
        for (int x = 0; x < a.Width; x++)
            if (a.Get(x, y) != b.Get(x, y)) return false;
        return true;
    }
}