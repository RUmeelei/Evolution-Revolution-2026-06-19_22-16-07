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

    private UnitManager unitManager;
    
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

        if (unitManager == null)
        {
            unitManager = FindFirstObjectByType<UnitManager>();
        }

        unitManager.Tick(delta);

        // Debug.Log($"Tick: {tickCounter}, weight; {delta}");
    }
    private void ProcessSlowTick(float delta)
    {
        slowTickTimer += delta;

        if (slowTickTimer >= slowTickInterval)
        {
            OnSlowTick?.Invoke(delta);

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