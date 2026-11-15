using System;
using UnityEngine;

public enum BorderMode { Infinite, Symmetric }

public class Game : MonoBehaviour
{
    [Header("Start settings")]
    [SerializeField] private int defaultWidth = 64;
    [SerializeField] private int defaultHeight = 36;
    [SerializeField] private BorderMode defaultBorder = BorderMode.Infinite;
    [SerializeField, Range(0.5f, 60f)] private float ticksPerSecond = 10f;

    public const int MinSize = 5;
    public const int MaxSize = 500;
    public const float MinSpeed = 0.5f;
    public const float MaxSpeed = 60f;

    public Simulation Sim { get; private set; }
    public bool IsPlaying { get; private set; }
    public BorderMode CurrentBorderMode { get; private set; }

    public GridModel Grid
    {
        get { return Sim?.Grid; }
    }

    public float TicksPerSecond
    {
        get { return ticksPerSecond; }
    }

    public int Generation
    {
        get { return Sim?.Generation ?? 0; }
    }

    public event Action<GridModel> GridReplaced;
    public event Action GridChanged;
    public event Action<bool> PlayStateChanged;

    private float _accumulator;
    private const int MaxStepsPerFrame = 5;

    // Initial grid
    private void Awake()
    {
        CreateGrid(defaultWidth, defaultHeight, defaultBorder);
    }

    // New empty grid
    public void CreateGrid(int width, int height, BorderMode mode)
    {
        width = Mathf.Clamp(width, MinSize, MaxSize);
        height = Mathf.Clamp(height, MinSize, MaxSize);
        LoadGrid(new GridModel(width, height), mode);
    }

    // Replace grid
    public void LoadGrid(GridModel grid, BorderMode mode)
    {
        Pause();

        if (Sim != null)
        {
            Sim.OnGridChanged -= RaiseGridChanged;
        }

        CurrentBorderMode = mode;
        Sim = new Simulation(grid, CreateRule(mode));
        Sim.OnGridChanged += RaiseGridChanged;
        _accumulator = 0f;

        GridReplaced?.Invoke(grid);
        GridChanged?.Invoke();
    }

    // Change border mode
    public void SetBorderMode(BorderMode mode)
    {
        CurrentBorderMode = mode;
        Sim.BorderRule = CreateRule(mode);
    }

    // Mode to rule
    private IBorderRule CreateRule(BorderMode mode)
    {
        if (mode == BorderMode.Symmetric)
        {
            return new WrapBorderRule();
        }
        return new DeadBorderRule();
    }

    // Start playback
    public void Play()
    {
        if (IsPlaying)
        {
            return;
        }
        IsPlaying = true;
        _accumulator = 0f;
        PlayStateChanged?.Invoke(true);
    }

    // Stop playback
    public void Pause()
    {
        if (!IsPlaying)
        {
            return;
        }
        IsPlaying = false;
        PlayStateChanged?.Invoke(false);
    }

    // Play/pause switch
    public void TogglePlay()
    {
        if (IsPlaying)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }

    // Single step
    public void StepOnce()
    {
        Pause();
        Sim.Step();
    }

    // Set speed
    public void SetSpeed(float newTicksPerSecond)
    {
        ticksPerSecond = Mathf.Clamp(newTicksPerSecond, MinSpeed, MaxSpeed);
    }

    // Timed ticks
    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        _accumulator += Time.deltaTime;
        float interval = 1f / ticksPerSecond;
        int steps = 0;

        while (_accumulator >= interval && steps < MaxStepsPerFrame)
        {
            Sim.Step();
            _accumulator -= interval;
            steps++;
        }

        if (steps == MaxStepsPerFrame)
        {
            _accumulator = 0f;
        }
    }

    // Flip cell
    public void ToggleCell(int x, int y)
    {
        if (!Grid.InBounds(x, y))
        {
            return;
        }
        Grid.Toggle(x, y);
        Sim.NotifyChanged();
    }

    // Set cell
    public void SetCell(int x, int y, bool alive)
    {
        if (!Grid.InBounds(x, y))
        {
            return;
        }
        if (Grid.Get(x, y) == alive)
        {
            return;
        }
        Grid.Set(x, y, alive);
        Sim.NotifyChanged();
    }

    // Clear grid
    public void Clear()
    {
        Grid.Clear();
        Sim.ResetGeneration();
        Sim.NotifyChanged();
    }

    // Forward event
    private void RaiseGridChanged()
    {
        GridChanged?.Invoke();
    }
}