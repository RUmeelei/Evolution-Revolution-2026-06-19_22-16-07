using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ContextMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject contextMenuPanel;
    [SerializeField] private TextMeshProUGUI contextMenuTitle;
    [SerializeField] private Button createUnitButton;
    [SerializeField] private Button buildFarmButton;
    [SerializeField] private Button buildMarketButton;
    [SerializeField] private Button buildWallButton;
    [SerializeField] private Button buildTempleButton;
    [SerializeField] private Button infoButton;

    private SelectionManager selectionManager;

    private TileManager tileManager;

    private FactionManager factionManager;

    private int playerFaction = -1;

    void Awake()
    {
        GameManager.RegisterContextMenuManager(this);
    }

    void Start()
    {
        selectionManager = FindFirstObjectByType<SelectionManager>();

        tileManager = GameManager.TileManager;

        factionManager = GameManager.FactionManager;
        
        for (int i = 0; i < factionManager.FactionCount; i++)
        {
            if (factionManager.IsPlayerFaction(i))
            {
                playerFaction = i; break;
            }
        }
        
        if (infoButton != null) infoButton.onClick.AddListener(ShowInfo);

        contextMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2Int tilePos = tileManager.WorldToTile(worldPos);

            TileData tile = tileManager.GetTile(tilePos.x, tilePos.y);

            if (tile.factionId == playerFaction)
            {
                selectionManager?.SetLastClickedTile(tilePos);
                
                ShowMenu(Input.mousePosition, tilePos);
            }
            else
            {
                HideMenu();
            }
        }
        
        if (Input.GetMouseButtonDown(0) && contextMenuPanel.activeSelf)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(contextMenuPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                HideMenu();
            }
        }
    }

    void ShowMenu(Vector2 screenPos, Vector2Int tilePos)
    {
        contextMenuPanel.SetActive(true);

        Vector2 offset = new Vector2(20f, -20f);

        contextMenuPanel.transform.position = screenPos + offset;

        contextMenuTitle.text = $"Тайл ({tilePos.x}, {tilePos.y})";
    }

    void HideMenu()
    {
        contextMenuPanel.SetActive(false);
    }

    void ShowInfo()
    {
        Vector2Int? tilePos = selectionManager?.GetLastClickedTile();

        if (!tilePos.HasValue) return;

        TileData tile = tileManager.GetTile(tilePos.Value.x, tilePos.Value.y);

        Debug.Log($"Тайл ({tilePos.Value.x}, {tilePos.Value.y}): {tile.tileType}, еда: {tile.foodAmount}");

        HideMenu();
    }
}