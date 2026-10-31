// Assets/Scripts/tests/DebugGridView.cs
// AFFICHAGE TEMPORAIRE pour tester le moteur avant que l'affichage d'Ilies soit prêt.
// À mettre sur le même GameObject que Game, puis lancer Play.
// Utilise OnGUI : marche sans Canvas, sans caméra spéciale et sans réglage d'Input System.
//
// Contrôles :
//   Espace       = Play / Pause
//   Flèche droite = 1 génération (pas-à-pas)
//   + / -        = plus vite / moins vite
//   B            = changer le mode de bord (Infini / Symétrique)
//   R            = remplir au hasard
//   G            = poser un planeur
//   C            = tout effacer
//   S            = sauvegarder la grille en PNG
//   L            = charger la dernière sauvegarde
//   O            = ouvrir le dossier des sauvegardes
//   Clic gauche  = allumer / éteindre une cellule (maintenir pour peindre)

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
    private static readonly Color32 Dead  = new Color32(0, 0, 0, 255);

    private void Start()
    {
        if (controller == null) controller = GetComponent<Game>();
        if (controller == null)
        {
            Debug.LogError("DebugGridView : mets ce script sur le même objet que Game.");
            enabled = false;
            return;
        }

        controller.GridReplaced += OnGridReplaced;
        controller.GridChanged  += Redraw;

        // La grille a déjà été créée dans Awake du controller : on la lit directement.
        OnGridReplaced(controller.Grid);
        if (randomFillAtStart) RandomFill();
    }

    private void OnDestroy()
    {
        if (controller == null) return;
        controller.GridReplaced -= OnGridReplaced;
        controller.GridChanged  -= Redraw;
    }

    // ---------- Texture ----------

    private void OnGridReplaced(GridModel grid)
    {
        if (_texture != null) Destroy(_texture);
        _texture = new Texture2D(grid.Width, grid.Height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,   // pixels nets, pas de flou
            wrapMode   = TextureWrapMode.Clamp
        };
        _pixels = new Color32[grid.Width * grid.Height];
        Redraw();
    }

    private void Redraw()
    {
        GridModel g = controller.Grid;
        if (g == null || _texture == null) return;

        for (int y = 0; y < g.Height; y++)
        for (int x = 0; x < g.Width; x++)
            _pixels[y * g.Width + x] = g.Get(x, y) ? Alive : Dead;

        _texture.SetPixels32(_pixels);
        _texture.Apply();
    }

    // ---------- Actions de test ----------

    private void RandomFill()
    {
        GridModel g = controller.Grid;
        var rng = new System.Random(randomSeed + _seedOffset++);
        controller.Clear();
        for (int y = 0; y < g.Height; y++)
        for (int x = 0; x < g.Width; x++)
            if (rng.NextDouble() < randomDensity) g.Set(x, y, true);
        controller.Sim.NotifyChanged();
    }

    private void PlaceGlider()
    {
        GridModel g = controller.Grid;
        int cx = g.Width / 2, cy = g.Height / 2;
        // Planeur qui part vers le bas à droite (y = 0 en bas de l'écran).
        controller.SetCell(cx,     cy + 1, true);
        controller.SetCell(cx + 1, cy,     true);
        controller.SetCell(cx - 1, cy - 1, true);
        controller.SetCell(cx,     cy - 1, true);
        controller.SetCell(cx + 1, cy - 1, true);
    }

    // ---------- Sauvegarde / chargement ----------

    private void Save()
    {
        try
        {
            string name = "grille_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string path = PngGridSerializer.Save(controller.Grid, name);
            _message = "Sauvegardé : " + Path.GetFileName(path);
            Debug.Log("Sauvegarde : " + path);
        }
        catch (Exception ex)
        {
            _message = "Erreur de sauvegarde : " + ex.Message;
            Debug.LogError(ex);
        }
    }

    private void LoadLatest()
    {
        try
        {
            string folder = PngGridSerializer.SaveFolder;
            string latest = Directory.Exists(folder)
                ? new DirectoryInfo(folder).GetFiles("*.png")
                    .OrderByDescending(f => f.LastWriteTime)
                    .Select(f => f.FullName)
                    .FirstOrDefault()
                : null;

            if (latest == null)
            {
                _message = "Aucune sauvegarde trouvée (appuie d'abord sur S).";
                return;
            }

            GridModel loaded = PngGridSerializer.Load(latest);
            controller.LoadGrid(loaded, controller.CurrentBorderMode); // garde le mode de bord actuel
            _message = "Chargé : " + Path.GetFileName(latest);
        }
        catch (Exception ex)
        {
            _message = "Erreur de chargement : " + ex.Message;
            Debug.LogError(ex);
        }
    }

    private void OpenSaveFolder()
    {
        Directory.CreateDirectory(PngGridSerializer.SaveFolder);
        Application.OpenURL("file:///" + PngGridSerializer.SaveFolder.Replace('\\', '/'));
        _message = "Dossier : " + PngGridSerializer.SaveFolder;
    }

    // ---------- Affichage + entrées ----------

    private void OnGUI()
    {
        if (_texture == null) return;
        GridModel g = controller.Grid;

        HandleInput(Event.current, g);

        // Place la grille au centre, en gardant des cellules carrées.
        const float top = 80f, margin = 10f;
        float availW = Screen.width - 2 * margin;
        float availH = Screen.height - top - margin;
        float cell = Mathf.Min(availW / g.Width, availH / g.Height);
        float w = cell * g.Width, h = cell * g.Height;
        _gridRect = new Rect((Screen.width - w) / 2f, top + (availH - h) / 2f, w, h);

        GUI.DrawTexture(_gridRect, _texture);

        string etat = controller.IsPlaying ? "LECTURE" : "PAUSE";
        GUI.Label(new Rect(10, 5, Screen.width - 20, 25),
            $"{etat}   Génération : {controller.Generation}   Vivantes : {g.CountAlive()}   " +
            $"Bord : {controller.CurrentBorderMode}   Vitesse : {controller.TicksPerSecond:0.#} /s   Taille : {g.Width}×{g.Height}");
        GUI.Label(new Rect(10, 28, Screen.width - 20, 25),
            "Espace = Play/Pause   Flèche droite = pas-à-pas   +/- = vitesse   B = bord   R = hasard   G = planeur   C = effacer   " +
            "S = sauver   L = charger   O = dossier   Clic = cellule");
        GUI.Label(new Rect(10, 51, Screen.width - 20, 25), _message);
    }

    private void HandleInput(Event e, GridModel g)
    {
        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.Space:       controller.TogglePlay(); break;
                case KeyCode.RightArrow:  controller.StepOnce(); break;
                case KeyCode.KeypadPlus:
                case KeyCode.Plus:
                case KeyCode.Equals:      controller.SetSpeed(controller.TicksPerSecond * 1.5f); break;
                case KeyCode.KeypadMinus:
                case KeyCode.Minus:       controller.SetSpeed(controller.TicksPerSecond / 1.5f); break;
                case KeyCode.B:
                    controller.SetBorderMode(controller.CurrentBorderMode == BorderMode.Infini
                        ? BorderMode.Symetrique : BorderMode.Infini);
                    break;
                case KeyCode.R:           RandomFill(); break;
                case KeyCode.G:           PlaceGlider(); break;
                case KeyCode.C:           controller.Clear(); break;
                case KeyCode.S:           Save(); break;
                case KeyCode.L:           LoadLatest(); break;
                case KeyCode.O:           OpenSaveFolder(); break;
                default: return;
            }
            e.Use();
            return;
        }

        if (e.button != 0) return;
        if (e.type != EventType.MouseDown && e.type != EventType.MouseDrag) return;
        if (!_gridRect.Contains(e.mousePosition)) return;

        float cell = _gridRect.width / g.Width;
        int x = Mathf.FloorToInt((e.mousePosition.x - _gridRect.x) / cell);
        int y = Mathf.FloorToInt((_gridRect.yMax - e.mousePosition.y) / cell); // l'écran GUI a y vers le bas

        if (e.type == EventType.MouseDown)
            _paintValue = !g.Get(x, y);   // le 1er clic décide si on allume ou si on éteint

        controller.SetCell(x, y, _paintValue);
        e.Use();
    }
}