using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ContextMenuManager : MonoBehaviour
{
    [Header("Own UI")]
    [SerializeField] private GameObject ownContextMenuPanel;
    [SerializeField] private TextMeshProUGUI contextMenuTitle;
    [SerializeField] private Button mobilizeUnitButton;
    [SerializeField] private Button recruitUnitButton;
    [SerializeField] private Button buildFarmButton;
    [SerializeField] private Button buildMarketButton;
    [SerializeField] private Button buildWallButton;
    [SerializeField] private Button buildTempleButton;
    [SerializeField] private Button ownInfoButton;

    [Header("Other UI")]
    [SerializeField] private GameObject otherContextMenuPanel;
    [SerializeField] private TextMeshProUGUI otherContextMenuTitle;
    [SerializeField] private Button otherOpenDiplomacyButton;
    [SerializeField] private Button otherInfoButton;

    [Header("Refs")]
    [SerializeField] private DiplomacyUI diplomacyUI;

    private Vector2 offset = new Vector2(100f, -150f);

    private SelectionManager selectionManager;

    private TileManager tileManager;

    private FactionManager factionManager;

    private SpawnManager spawnManager;

    private int playerFaction = -1;

    void Awake()
    {
        GameManager.RegisterContextMenuManager(this);
    }

    void Start()
    {
        selectionManager = GameManager.SelectionManager;

        tileManager = GameManager.TileManager;

        factionManager = GameManager.FactionManager;

        spawnManager = GameManager.SpawnManager;

        for (int i = 0; i < factionManager.FactionCount; i++)
        {
            if (factionManager.IsPlayerFaction(i))
            {
                playerFaction = i; break;
            }
        }
        
        if (ownInfoButton != null) ownInfoButton.onClick.AddListener(ShowInfo);

        if (otherInfoButton != null) otherInfoButton.onClick.AddListener(ShowInfo);

        if (diplomacyUI != null && otherOpenDiplomacyButton != null) otherOpenDiplomacyButton.onClick.AddListener(OpenDiplomacy); else if (otherOpenDiplomacyButton != null) otherOpenDiplomacyButton.interactable = false;

        if (spawnManager != null && mobilizeUnitButton != null) mobilizeUnitButton.onClick.AddListener(() => SpawnUnit(reg : false)); else if (mobilizeUnitButton != null) mobilizeUnitButton.interactable = false;

        if (spawnManager != null && recruitUnitButton != null) recruitUnitButton.onClick.AddListener(() => SpawnUnit(reg : true)); else if (recruitUnitButton != null) recruitUnitButton.interactable = false;

        if (buildFarmButton != null) buildFarmButton.onClick.AddListener(() => BuildBuilding(BuildingType.Farm));

        if (buildMarketButton != null) buildMarketButton.onClick.AddListener(() => BuildBuilding(BuildingType.Market));

        if (buildWallButton != null) buildWallButton.onClick.AddListener(() => BuildBuilding(BuildingType.Wall));

        if (buildTempleButton != null) buildTempleButton.onClick.AddListener(() => BuildBuilding(BuildingType.Temple));

        ownContextMenuPanel.SetActive(false);
        otherContextMenuPanel.SetActive(false);
    }

    void Update()
    {
        // if ((Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)) && (ownContextMenuPanel.activeSelf || otherContextMenuPanel.activeSelf))
        // {
        //     HideOwnMenu();
        //     HideOtherMenu();
        // }
        // else if (Input.GetMouseButtonDown(0) && (ownContextMenuPanel.activeSelf || otherContextMenuPanel.activeSelf))
        // {
        //     if (!RectTransformUtility.RectangleContainsScreenPoint(ownContextMenuPanel.GetComponent<RectTransform>(), Input.mousePosition) && !RectTransformUtility.RectangleContainsScreenPoint(otherContextMenuPanel.GetComponent<RectTransform>(), Input.mousePosition))
        //     {
        //         HideOwnMenu();
        //         HideOtherMenu();
        //     }
        // }
        if (!RectTransformUtility.RectangleContainsScreenPoint(ownContextMenuPanel.GetComponent<RectTransform>(), Input.mousePosition) && !RectTransformUtility.RectangleContainsScreenPoint(otherContextMenuPanel.GetComponent<RectTransform>(), Input.mousePosition))
        {
            HideOwnMenu();
            HideOtherMenu();
        }
    }

    public void Check(Vector2Int? tilePos = null)
    {
        if (tilePos == null) return;

        TileData tile = tileManager.GetTile(tilePos.Value.x, tilePos.Value.y);
         
        if (tile.factionId == playerFaction)
        {
            selectionManager?.SetLastClickedTile(tileManager.WorldToTile(tilePos.Value));
            
            ShowOwnMenu(Input.mousePosition, tileManager.WorldToTile(tilePos.Value));
            HideOtherMenu();
        }
        else if (tile.factionId != -1)
        {
            selectionManager?.SetLastClickedTile(tileManager.WorldToTile(tilePos.Value));
         
            ShowOtherMenu(Input.mousePosition, tileManager.WorldToTile(tilePos.Value));
            HideOwnMenu();
        }
        else
        {
            HideOwnMenu();
            HideOtherMenu();
        }
    }

    void ShowOwnMenu(Vector2 screenPos, Vector2Int tilePos)
    {
        ownContextMenuPanel.SetActive(true);

        ownContextMenuPanel.transform.position = screenPos;

        contextMenuTitle.text = $"Тайл ({tilePos.x}, {tilePos.y})";
    }

    void HideOwnMenu()
    {
        ownContextMenuPanel.SetActive(false);
    }

    void ShowOtherMenu(Vector2 screenPos, Vector2Int tilePos)
    {
        otherContextMenuPanel.SetActive(true);

        otherContextMenuPanel.transform.position = screenPos;

        otherContextMenuTitle.text = $"Тайл ({tilePos.x}, {tilePos.y})";
    }

    void HideOtherMenu()
    {
        otherContextMenuPanel.SetActive(false);
    }

    void SpawnUnit(bool reg = false)
    {
        if (selectionManager == null || tileManager == null || spawnManager == null) return;

        Vector2Int? tilePos = selectionManager?.GetLastClickedTile();

        if (!tilePos.HasValue) return;

        TileData tile = tileManager.GetTile(tilePos.Value.x, tilePos.Value.y);

        Vector2 targetPos = new Vector2(tilePos.Value.x * tileManager.tileSize + tileManager.tileSize / 2f, tilePos.Value.y * tileManager.tileSize + tileManager.tileSize / 2f);

        if (tile.factionId == playerFaction)
        {
            if (reg) spawnManager.RecruitUnit(targetPos, playerFaction); else spawnManager.MobilizeUnit(targetPos, playerFaction);
        }

        HideOwnMenu();
        HideOtherMenu();
    }

    void BuildBuilding(BuildingType type)
    {
        Vector2Int? tilePos = selectionManager?.GetLastClickedTile();

        if (!tilePos.HasValue) return;

        BuildingManager buildingManager = GameManager.BuildingManager;

        if (buildingManager != null)
        {
            buildingManager.BuildBuilding(tilePos.Value, type, playerFaction);
        }

        HideOwnMenu();
        HideOtherMenu();
    }

    void OpenDiplomacy()
    {
        if (diplomacyUI != null && selectionManager != null)
        {
            diplomacyUI.OpenDiplomacyPanel();
        }

        HideOwnMenu();
        HideOtherMenu();
    }

    void ShowInfo()
    {
        Vector2Int? tilePos = selectionManager?.GetLastClickedTile();

        if (!tilePos.HasValue) return;

        TileData tile = tileManager.GetTile(tilePos.Value.x, tilePos.Value.y);

        Debug.Log($"Тайл ({tilePos.Value.x}, {tilePos.Value.y}): {tile.tileType}, еда: {tile.foodAmount}");

        HideOwnMenu();
        HideOtherMenu();
    }
}