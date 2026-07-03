using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public static TileManager TileManager { get; private set; }
    public static UnitManager UnitManager { get; private set; }
    public static FactionManager FactionManager { get; private set; }
    public static EconomyManager EconomyManager { get; private set; }
    public static PoliticsManager PoliticsManager { get; private set; }
    public static DiplomacyManager DiplomacyManager { get; private set; }
    public static AIManager AIManager { get; private set; }
    public static SpawnManager SpawnManager { get; private set; }
    public static WinConditionManager WinConditionManager { get; private set; }
    public static SimulationManager SimulationManager { get; private set; }
    public static RegionManager RegionManager { get; private set; }
    // public static BuildingManager BuildingManager { get; private set; }
    public static SelectionManager SelectionManager { get; private set; }
    public static CameraManager CameraManager { get; private set; }
    public static AudioManager AudioManager { get; private set; }
    public static TilemapVisualManager TilemapVisualManager { get; private set; }
    public static UnitVisualManager UnitVisualManager { get; private set; }
    public static UIManager UIManager { get; private set; }
    public static ContextMenuManager ContextMenuManager { get; private set; }
    
    public static void RegisterTileManager(TileManager manager) => TileManager = manager;
    public static void RegisterUnitManager(UnitManager manager) => UnitManager = manager;
    public static void RegisterFactionManager(FactionManager manager) => FactionManager = manager;
    public static void RegisterEconomyManager(EconomyManager manager) => EconomyManager = manager;
    public static void RegisterPoliticsManager(PoliticsManager manager) => PoliticsManager = manager;
    public static void RegisterDiplomacyManager(DiplomacyManager manager) => DiplomacyManager = manager;
    public static void RegisterAIManager(AIManager manager) => AIManager = manager;
    public static void RegisterSpawnManager(SpawnManager manager) => SpawnManager = manager;
    public static void RegisterWinConditionManager(WinConditionManager manager) => WinConditionManager = manager;
    public static void RegisterSimulationManager(SimulationManager manager) => SimulationManager = manager;
    public static void RegisterRegionManager(RegionManager manager) => RegionManager = manager;
    // public static void RegisterBuildingManager(BuildingManager manager) => BuildingManager = manager;
    public static void RegisterSelectionManager(SelectionManager manager) => SelectionManager = manager;
    public static void RegisterCameraManager(CameraManager manager) => CameraManager = manager;
    public static void RegisterAudioManager(AudioManager manager) => AudioManager = manager;
    public static void RegisterTilemapVisualManager(TilemapVisualManager manager) => TilemapVisualManager = manager;
    public static void RegisterUnitVisualManager(UnitVisualManager manager) => UnitVisualManager = manager;
    public static void RegisterUIManager(UIManager manager) => UIManager = manager;
    public static void RegisterContextMenuManager(ContextMenuManager manager) => ContextMenuManager = manager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}