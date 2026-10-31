
using System.IO;
using UnityEngine;

public static class PngGridSerializer
{
    private static readonly Color32 AliveColor = new Color32(255, 255, 255, 255);
    private static readonly Color32 DeadColor  = new Color32(0, 0, 0, 255);

    public static string SaveFolder => Path.Combine(Application.persistentDataPath, "Saves");

    //  Grille to PNG

    public static byte[] Encode(GridModel grid)
    {
        int w = grid.Width, h = grid.Height;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
            pixels[y * w + x] = grid.Get(x, y) ? AliveColor : DeadColor;

        tex.SetPixels32(pixels);
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        Release(tex);
        return png;
    }

    // PNG to grille

    public static GridModel Decode(byte[] png)
    {
        var tex = new Texture2D(2, 2); // la taille est remplacée par LoadImage
        if (!tex.LoadImage(png))
        {
            Release(tex);
            throw new InvalidDataException("Ce fichier n'est pas une image PNG lisible.");
        }

        int w = tex.width, h = tex.height;
        Color32[] pixels = tex.GetPixels32();
        Release(tex);

        var grid = new GridModel(w, h);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            Color32 c = pixels[y * w + x];
            // Seuil de luminosité : robuste si l'image a été retouchée / compressée.
            grid.Set(x, y, (c.r + c.g + c.b) / 3 > 127);
        }
        return grid;
    }

    //  Fichiers

    public static string Save(GridModel grid, string fileName)
    {
        Directory.CreateDirectory(SaveFolder);
        if (!fileName.EndsWith(".png")) fileName += ".png";
        string path = Path.Combine(SaveFolder, fileName);
        File.WriteAllBytes(path, Encode(grid));
        return path;
    }

    public static GridModel Load(string path) => Decode(File.ReadAllBytes(path));

    private static void Release(UnityEngine.Object obj)
    {
        if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
        else UnityEngine.Object.DestroyImmediate(obj);
    }
}