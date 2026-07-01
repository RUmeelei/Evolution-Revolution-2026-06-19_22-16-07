using System;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private bool isRunning = false;

    [SerializeField] private int simulationSpeed = 1;
    [SerializeField] private int ticksPerSecond = 10;

    public bool Running => isRunning;

    public int SimulationSpeed => simulationSpeed;

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

    private FactionManager factionManager;

    private UnitManager unitManager;
    private UnitVisualManager unitVisualManager;

    private TileManager tileManager;
    private TilemapVisualManager tileVisualManager;
    
    private RegionManager regionManager;
    public RegionManager RegionManager => regionManager;

    private EconomyManager economyManager;

    private AIManager aiManager;

    private SpawnManager spawnManager;

    private WinConditionManager winConditionManager;

    private PoliticsManager politicsManager;

    private DiplomacyManager diplomacyManager;

    void Awake()
    {
        factionManager = FindFirstObjectByType<FactionManager>();

        factionManager.Initialize();

        tileManager = FindFirstObjectByType<TileManager>();
        tileVisualManager = FindFirstObjectByType<TilemapVisualManager>();

        tileManager.Initialize();

        regionManager = new RegionManager();
        regionManager.Initialize(20);
        
        tileVisualManager.Initialize(tileManager);
        tileVisualManager.RedrawAllTiles();

        unitManager = FindFirstObjectByType<UnitManager>();
        unitVisualManager = FindFirstObjectByType<UnitVisualManager>();

        unitManager.Initialize(1000);
        unitVisualManager.Initialize(unitManager);
        
        economyManager = FindFirstObjectByType<EconomyManager>();

        economyManager.Initialize();

        aiManager = FindFirstObjectByType<AIManager>();

        aiManager.Initialize();

        spawnManager = FindFirstObjectByType<SpawnManager>();

        spawnManager.Initialize();

        winConditionManager = FindFirstObjectByType<WinConditionManager>();

        winConditionManager.Initialize();

        politicsManager = FindFirstObjectByType<PoliticsManager>();

        politicsManager.Initialize();

        diplomacyManager = FindFirstObjectByType<DiplomacyManager>();

        diplomacyManager.Initialize();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) isRunning = !isRunning;

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

            if (economyManager == null)
            {
                economyManager = FindFirstObjectByType<EconomyManager>();
            }

            economyManager.ProcessEconomy(delta);

            if (aiManager == null)
            {
                aiManager = FindFirstObjectByType<AIManager>();
            }

            aiManager.ProcessAI(delta);

            // if (unitManager.Humans.Length > 0) unitManager.RandomMoveUnit(unitManager.GetRandomUnitIndex());

            if (tileManager == null)
            {
                tileManager = FindFirstObjectByType<TileManager>();
            }

            tileManager.UpdateTileOwnership();

            regionManager.UpdateRegions(tileManager);

            if (politicsManager == null)
            {
                politicsManager = FindFirstObjectByType<PoliticsManager>();
            }

            politicsManager.UpdatePolitics(delta);

            slowTickTimer = 0f;
        }
    }
    private void ProcessEpicTick(float delta)
    {
        epicTickTimer += delta;
        
        if (epicTickTimer >= epicTickInterval)
        {
            OnEpicTick?.Invoke(delta);

            if (winConditionManager == null)
            {
                winConditionManager = FindFirstObjectByType<WinConditionManager>();
            }
            
            winConditionManager.CheckVictory();

            if (politicsManager == null)
            {
                politicsManager = FindFirstObjectByType<PoliticsManager>();
            }

            politicsManager.ProcessConstruction();

            epicTickTimer = 0f;
        }
    }

    public void StopSimulation()
    {
        isRunning = false;
    }

    public void PauseSimulation()
    {
        isRunning = false;
    }

    public void ResumeSimulation()
    {
        isRunning = true;
    }

    public void SetSimulationSpeed(int speed)
    {
        simulationSpeed = Mathf.Max(1, speed);
    }

    public void IncreaseSimulationSpeed()
    {
        simulationSpeed = Mathf.Min(simulationSpeed + 1, 10);
    }

    public void DecreaseSimulationSpeed()
    {
        simulationSpeed = Mathf.Max(1, simulationSpeed - 1);
    }
}