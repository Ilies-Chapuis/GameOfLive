using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class DebugGridView : MonoBehaviour
{
    [SerializeField] private Game controller;
    [SerializeField] private bool randomFillAtStart = true;
    [SerializeField, Range(0f, 1f)] private float randomDensity = 0.3f;
    [SerializeField] private int randomSeed = 42;

    private Texture2D _texture;
    private Color32[] _pixels;
    private Rect _gridRect;
    private bool _paintValue;
    private int _seedOffset;
    private string _message = "";

    private static readonly Color32 Alive = new Color32(255, 255, 255, 255);
    private static readonly Color32 Dead = new Color32(0, 0, 0, 255);

    // Hook events
    private void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<Game>();
        }
        if (controller == null)
        {
            Debug.LogError("DebugGridView: put this script on the same object as Game.");
            enabled = false;
            return;
        }

        controller.GridReplaced += OnGridReplaced;
        controller.GridChanged += Redraw;

        OnGridReplaced(controller.Grid);
        if (randomFillAtStart)
        {
            RandomFill();
        }
    }

    // Unhook events
    private void OnDestroy()
    {
        if (controller == null)
        {
            return;
        }
        controller.GridReplaced -= OnGridReplaced;
        controller.GridChanged -= Redraw;
    }

    // Rebuild texture
    private void OnGridReplaced(GridModel grid)
    {
        if (_texture != null)
        {
            Destroy(_texture);
        }
        _texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        _pixels = new Color32[grid.Width * grid.Height];
        Redraw();
    }

    // Repaint texture
    private void Redraw()
    {
        GridModel g = controller.Grid;
        if (g == null || _texture == null)
        {
            return;
        }

        for (int y = 0; y < g.Height; y++)
        {
            for (int x = 0; x < g.Width; x++)
            {
                if (g.Get(x, y))
                {
                    _pixels[y * g.Width + x] = Alive;
                }
                else
                {
                    _pixels[y * g.Width + x] = Dead;
                }
            }
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply();
    }

    // Random fill
    private void RandomFill()
    {
        GridModel g = controller.Grid;
        System.Random rng = new System.Random(randomSeed + _seedOffset);
        _seedOffset++;
        controller.Clear();
        for (int y = 0; y < g.Height; y++)
        {
            for (int x = 0; x < g.Width; x++)
            {
                if (rng.NextDouble() < randomDensity)
                {
                    g.Set(x, y, true);
                }
            }
        }
        controller.Sim.NotifyChanged();
    }

    // Spawn glider
    private void PlaceGlider()
    {
        GridModel g = controller.Grid;
        int cx = g.Width / 2;
        int cy = g.Height / 2;
        controller.SetCell(cx, cy + 1, true);
        controller.SetCell(cx + 1, cy, true);
        controller.SetCell(cx - 1, cy - 1, true);
        controller.SetCell(cx, cy - 1, true);
        controller.SetCell(cx + 1, cy - 1, true);
    }

    // Save PNG
    private void Save()
    {
        try
        {
            string name = "grid_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string path = PngGridSerializer.Save(controller.Grid, name);
            _message = "Saved: " + Path.GetFileName(path);
            Debug.Log("Saved: " + path);
        }
        catch (Exception ex)
        {
            _message = "Save error: " + ex.Message;
            Debug.LogError(ex);
        }
    }

    // Load latest PNG
    private void LoadLatest()
    {
        try
        {
            string folder = PngGridSerializer.SaveFolder;
            if (!Directory.Exists(folder))
            {
                _message = "No save found (press S first).";
                return;
            }

            FileInfo latest = new DirectoryInfo(folder)
                .GetFiles("*.png")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (latest == null)
            {
                _message = "No save found (press S first).";
                return;
            }

            GridModel loaded = PngGridSerializer.Load(latest.FullName);
            controller.LoadGrid(loaded, controller.CurrentBorderMode);
            _message = "Loaded: " + latest.Name;
        }
        catch (Exception ex)
        {
            _message = "Load error: " + ex.Message;
            Debug.LogError(ex);
        }
    }

    // Open saves folder
    private void OpenSaveFolder()
    {
        Directory.CreateDirectory(PngGridSerializer.SaveFolder);
        Application.OpenURL("file:///" + PngGridSerializer.SaveFolder.Replace('\\', '/'));
        _message = "Folder: " + PngGridSerializer.SaveFolder;
    }

    // Draw UI
    private void OnGUI()
    {
        if (_texture == null)
        {
            return;
        }
        GridModel g = controller.Grid;

        float ui = Mathf.Max(1f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.Scale(new Vector3(ui, ui, 1f));
        float sw = Screen.width / ui;
        float sh = Screen.height / ui;

        HandleInput(Event.current, g);

        float top = 80f;
        float margin = 10f;
        float availW = sw - 2 * margin;
        float availH = sh - top - margin;
        float cell = Mathf.Min(availW / g.Width, availH / g.Height);
        float w = cell * g.Width;
        float h = cell * g.Height;
        _gridRect = new Rect((sw - w) / 2f, top + (availH - h) / 2f, w, h);

        GUI.DrawTexture(_gridRect, _texture);

        string state = "PAUSED";
        if (controller.IsPlaying)
        {
            state = "PLAYING";
        }

        GUI.Label(new Rect(10, 5, sw - 20, 25),
            state + "   Generation: " + controller.Generation + "   Alive: " + g.CountAlive() +
            "   Border: " + controller.CurrentBorderMode + "   Speed: " + controller.TicksPerSecond.ToString("0.#") +
            "/s   Size: " + g.Width + "x" + g.Height);
        GUI.Label(new Rect(10, 28, sw - 20, 25),
            "Space = Play/Pause   Right = step   +/- = speed   B = border   R = random   G = glider   C = clear   " +
            "S = save   L = load   O = folder   Click = cell");
        GUI.Label(new Rect(10, 51, sw - 20, 25), _message);
    }

    // Keys and mouse
    private void HandleInput(Event e, GridModel g)
    {
        if (e.type == EventType.KeyDown)
        {
            HandleKey(e.keyCode, e);
            return;
        }

        if (e.button != 0)
        {
            return;
        }
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag)
        {
            return;
        }
        if (!_gridRect.Contains(e.mousePosition))
        {
            return;
        }

        float cell = _gridRect.width / g.Width;
        int x = Mathf.FloorToInt((e.mousePosition.x - _gridRect.x) / cell);
        int y = Mathf.FloorToInt((_gridRect.yMax - e.mousePosition.y) / cell);

        if (e.type == EventType.MouseDown)
        {
            _paintValue = !g.Get(x, y);
        }

        controller.SetCell(x, y, _paintValue);
        e.Use();
    }

    // Keyboard actions
    private void HandleKey(KeyCode key, Event e)
    {
        if (key == KeyCode.Space)
        {
            controller.TogglePlay();
        }
        else if (key == KeyCode.RightArrow)
        {
            controller.StepOnce();
        }
        else if (key == KeyCode.KeypadPlus || key == KeyCode.Plus || key == KeyCode.Equals)
        {
            controller.SetSpeed(controller.TicksPerSecond * 1.5f);
        }
        else if (key == KeyCode.KeypadMinus || key == KeyCode.Minus)
        {
            controller.SetSpeed(controller.TicksPerSecond / 1.5f);
        }
        else if (key == KeyCode.B)
        {
            if (controller.CurrentBorderMode == BorderMode.Infinite)
            {
                controller.SetBorderMode(BorderMode.Symmetric);
            }
            else
            {
                controller.SetBorderMode(BorderMode.Infinite);
            }
        }
        else if (key == KeyCode.R)
        {
            RandomFill();
        }
        else if (key == KeyCode.G)
        {
            PlaceGlider();
        }
        else if (key == KeyCode.C)
        {
            controller.Clear();
        }
        else if (key == KeyCode.S)
        {
            Save();
        }
        else if (key == KeyCode.L)
        {
            LoadLatest();
        }
        else if (key == KeyCode.O)
        {
            OpenSaveFolder();
        }
        else
        {
            return;
        }
        e.Use();
    }
}