using System;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private bool isRunning = false;

    [SerializeField] private int simulationSpeed = 1;
    [SerializeField] private int ticksPerSecond = 10;

    public event Action<float> OnTick;
    public event Action<float> OnSlowTick;
    public event Action<float> OnEpicTick;

    private float slowTickInterval = 1f;
    private float epicTickInterval = 5f;

    private int tickCounter = 0;

    private float tickTimer = 0f;
    private float slowTickTimer = 0f;
    private float epicTickTimer = 0f;

    public float LastTickTime { get; private set; }
    public float TickInterval => 1f / ticksPerSecond;

    private UnitManager unitManager;
    private UnitVisualManager unitVisualManager;

    void Awake()
    {
        unitManager = FindFirstObjectByType<UnitManager>();
        unitVisualManager = FindFirstObjectByType<UnitVisualManager>();

        unitManager.Initialize(50000);
        unitVisualManager.Initialize(unitManager);
    }
    
    void Update()
    {
        if (!isRunning) return;

        float tickInterval = 1f / Mathf.Max(1, ticksPerSecond);

        int maxTicksThisFrame = Mathf.CeilToInt(ticksPerSecond * 0.1f);
        int ticksThisFrame = 0;

        tickTimer += Time.deltaTime;

        while (tickTimer >= tickInterval && ticksThisFrame < maxTicksThisFrame)
        {
            float effectiveDelta = tickInterval * simulationSpeed;

            ProcessTick(effectiveDelta);

            ProcessSlowTick(effectiveDelta);

            ProcessEpicTick(effectiveDelta);

            tickTimer -= tickInterval;

            ticksThisFrame++;
        }

        if (ticksThisFrame >= maxTicksThisFrame) tickTimer = 0f;
    }
    private void ProcessTick(float delta)
    {
        tickCounter++;

        OnTick?.Invoke(delta);

        LastTickTime = Time.time;

        if (unitManager == null)
        {
            unitManager = FindFirstObjectByType<UnitManager>();
        }

        unitManager.Tick(delta);
    }
    private void ProcessSlowTick(float delta)
    {
        slowTickTimer += delta;

        if (slowTickTimer >= slowTickInterval)
        {
            OnSlowTick?.Invoke(delta);

            if (unitManager == null)
            {
                unitManager = FindFirstObjectByType<UnitManager>();
            }

            unitManager.RandomMoveUnit(unitManager.GetRandomUnitIndex());

            slowTickTimer = 0f;
        }
    }
    private void ProcessEpicTick(float delta)
    {
        epicTickTimer += delta;
        
        if (epicTickTimer >= epicTickInterval)
        {
            OnEpicTick?.Invoke(delta);

            epicTickTimer = 0f;
        }
    }
}