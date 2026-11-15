using System.IO;
using UnityEngine;

public static class PngGridSerializer
{
    private static readonly Color32 AliveColor = new Color32(255, 255, 255, 255);
    private static readonly Color32 DeadColor = new Color32(0, 0, 0, 255);

    public static string SaveFolder
    {
        get { return Path.Combine(Application.persistentDataPath, "Saves"); }
    }

    // Grid to PNG
    public static byte[] Encode(GridModel grid)
    {
        int w = grid.Width;
        int h = grid.Height;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (grid.Get(x, y))
                {
                    pixels[y * w + x] = AliveColor;
                }
                else
                {
                    pixels[y * w + x] = DeadColor;
                }
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        Release(tex);
        return png;
    }

    // PNG to grid
    public static GridModel Decode(byte[] png)
    {
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(png))
        {
            Release(tex);
            throw new InvalidDataException("This file is not a readable PNG image.");
        }

        int w = tex.width;
        int h = tex.height;
        Color32[] pixels = tex.GetPixels32();
        Release(tex);

        GridModel grid = new GridModel(w, h);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color32 c = pixels[y * w + x];
                int brightness = (c.r + c.g + c.b) / 3;
                grid.Set(x, y, brightness > 127);
            }
        }
        return grid;
    }

    // Write PNG file
    public static string Save(GridModel grid, string fileName)
    {
        Directory.CreateDirectory(SaveFolder);
        if (!fileName.EndsWith(".png"))
        {
            fileName += ".png";
        }
        string path = Path.Combine(SaveFolder, fileName);
        File.WriteAllBytes(path, Encode(grid));
        return path;
    }

    // Read PNG file
    public static GridModel Load(string path)
    {
        return Decode(File.ReadAllBytes(path));
    }

    // Free texture
    private static void Release(Object obj)
    {
        if (Application.isPlaying)
        {
            Object.Destroy(obj);
        }
        else
        {
            Object.DestroyImmediate(obj);
        }
    }
}