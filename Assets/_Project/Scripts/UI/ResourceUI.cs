using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI stoneText;
    [SerializeField] private TextMeshProUGUI woodText;

    private EconomyManager economyManager;
    private FactionManager factionManager;
    private SelectionManager selectionManager;
    private TileManager tileManager;
    private int playerFaction = -1;

    void Start()
    {
        economyManager = GameManager.EconomyManager;

        factionManager = GameManager.FactionManager;

        selectionManager = GameManager.SelectionManager;

        tileManager = GameManager.TileManager;
        
        for (int i = 0; i < factionManager.FactionCount; i++)
        {
            if (factionManager.IsPlayerFaction(i))
            {
                playerFaction = i; break;
            }
        }
    }

    void Update()
    {
        if (economyManager == null || factionManager == null || selectionManager == null || tileManager == null) return;
        
        if (foodText != null && goldText != null && stoneText != null && woodText != null)
        {
            float food = 0f;
            float gold = 0f;
            float stone = 0f;
            float wood = 0f;

            if (playerFaction != -1)
            {
                food = economyManager.GetFood(playerFaction);
                gold = economyManager.GetGold(playerFaction);
                stone = economyManager.GetStone(playerFaction);
                wood = economyManager.GetWood(playerFaction);
            }
            else if (selectionManager != null && selectionManager.GetLastClickedTile() != null)
            {
                food = economyManager.GetFood(tileManager.GetTile(selectionManager.GetLastClickedTile().Value.x, selectionManager.GetLastClickedTile().Value.y).factionId);
                gold = economyManager.GetGold(tileManager.GetTile(selectionManager.GetLastClickedTile().Value.x, selectionManager.GetLastClickedTile().Value.y).factionId);
                stone = economyManager.GetStone(tileManager.GetTile(selectionManager.GetLastClickedTile().Value.x, selectionManager.GetLastClickedTile().Value.y).factionId);
                wood = economyManager.GetWood(tileManager.GetTile(selectionManager.GetLastClickedTile().Value.x, selectionManager.GetLastClickedTile().Value.y).factionId);
            }

            foodText.text = $"Еда: {food:F0}";
            goldText.text = $"Золото: {gold:F0}";
            stoneText.text = $"Камень: {stone:F0}";
            woodText.text = $"Дерево: {wood:F0}";
        }
    }
}