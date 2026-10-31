
using System;
using UnityEngine;

public enum BorderMode { Infini, Symetrique }

public class Game : MonoBehaviour
{
    [Header("Valeurs par défaut au lancement")]
    [SerializeField] private int defaultWidth = 64;
    [SerializeField] private int defaultHeight = 36;
    [SerializeField] private BorderMode defaultBorder = BorderMode.Infini;
    [SerializeField, Range(0.5f, 60f)] private float ticksPerSecond = 10f;

    public const int MinSize = 5;
    public const int MaxSize = 500;
    public const float MinSpeed = 0.5f;
    public const float MaxSpeed = 60f;

    public Simulation Sim { get; private set; }
    public GridModel Grid => Sim?.Grid;
    public bool IsPlaying { get; private set; }
    public BorderMode CurrentBorderMode { get; private set; }
    public float TicksPerSecond => ticksPerSecond;
    public int Generation => Sim?.Generation ?? 0;

    /// Nex grille 
    public event Action<GridModel> GridReplaced;
    /// content in grille
    public event Action GridChanged;
    /// Play / Pause 
    public event Action<bool> PlayStateChanged;

    private float _accumulator;
    private const int MaxStepsPerFrame = 5; 

    private void Awake()
    {
        CreateGrid(defaultWidth, defaultHeight, defaultBorder);
    }

    // create/load

    public void CreateGrid(int width, int height, BorderMode mode)
    {
        width  = Mathf.Clamp(width,  MinSize, MaxSize);
        height = Mathf.Clamp(height, MinSize, MaxSize);
        LoadGrid(new GridModel(width, height), mode);
    }

    public void LoadGrid(GridModel grid, BorderMode mode)
    {
        Pause();

        if (Sim != null) Sim.OnGridChanged -= RaiseGridChanged;

        CurrentBorderMode = mode;
        Sim = new Simulation(grid, CreateRule(mode));
        Sim.OnGridChanged += RaiseGridChanged;
        _accumulator = 0f;

        GridReplaced?.Invoke(grid);
        GridChanged?.Invoke();
    }

    public void SetBorderMode(BorderMode mode)
    {
        CurrentBorderMode = mode;
        Sim.BorderRule = CreateRule(mode);
    }

    private static IBorderRule CreateRule(BorderMode mode)
        => mode == BorderMode.Symetrique ? new WrapBorderRule() : (IBorderRule)new DeadBorderRule();

    // time controller

    public void Play()
    {
        if (IsPlaying) return;
        IsPlaying = true;
        _accumulator = 0f;
        PlayStateChanged?.Invoke(true);
    }

    public void Pause()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        PlayStateChanged?.Invoke(false);
    }

    public void TogglePlay()
    {
        if (IsPlaying) Pause(); else Play();
    }

    /// frame/frame
    public void StepOnce()
    {
        Pause();
        Sim.Step();
    }

    public void SetSpeed(float newTicksPerSecond)
    {
        ticksPerSecond = Mathf.Clamp(newTicksPerSecond, MinSpeed, MaxSpeed);
    }

    private void Update()
    {
        if (!IsPlaying) return;

        _accumulator += Time.deltaTime;
        float interval = 1f / ticksPerSecond;
        int steps = 0;

        while (_accumulator >= interval && steps < MaxStepsPerFrame)
        {
            Sim.Step();
            _accumulator -= interval;
            steps++;
        }

        if (steps == MaxStepsPerFrame) _accumulator = 0f;
    }


    public void ToggleCell(int x, int y)
    {
        if (!Grid.InBounds(x, y)) return;
        Grid.Toggle(x, y);
        Sim.NotifyChanged();
    }

    public void SetCell(int x, int y, bool alive)
    {
        if (!Grid.InBounds(x, y) || Grid.Get(x, y) == alive) return;
        Grid.Set(x, y, alive);
        Sim.NotifyChanged();
    }

    public void Clear()
    {
        Grid.Clear();
        Sim.ResetGeneration();
        Sim.NotifyChanged();
    }

    private void RaiseGridChanged() => GridChanged?.Invoke();
}