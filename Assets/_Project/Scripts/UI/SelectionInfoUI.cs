using UnityEngine;
using TMPro;

public class SelectionInfoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI infoText;

    private SelectionManager selectionManager;
    private UnitManager unitManager;
    private TileManager tileManager;
    private FactionManager factionManager;

    void Start()
    {
        selectionManager = GameManager.SelectionManager;

        unitManager = GameManager.UnitManager;

        tileManager = GameManager.TileManager;

        factionManager = GameManager.FactionManager;

        if (selectionPanel != null) selectionPanel.SetActive(false);
    }

    void Update()
    {
        if (selectionManager == null || unitManager == null || tileManager == null || factionManager == null) return;

        int selectedUnitIndex = selectionManager.GetFirstSelectedUnit();

        Vector2Int? selectedTile = selectionManager.GetLastClickedTile();

        if (selectedUnitIndex >= 0)
        {
            HumanData unit = unitManager.Humans[selectedUnitIndex];

            string factionName = factionManager.GetFaction(unit.factionId)?.factionName ?? "Неизвестно";

            infoText.text = $"Юнит\nЗдоровье: {unit.hp:F0}/{unit.maxHp}\nСтамина: {unit.stamina:F0}/{unit.maxStamina}\nФракция: {factionName}";

            selectionPanel.SetActive(true);
        }
        else if (selectedTile.HasValue)
        {
            TileData tile = tileManager.GetTile(selectedTile.Value.x, selectedTile.Value.y);

            string ownerName = "Нейтральный";

            if (tile.factionId >= 0) ownerName = factionManager.GetFaction(tile.factionId)?.factionName ?? "Неизвестно";

            infoText.text = $"Тайл ({selectedTile.Value.x},{selectedTile.Value.y})\nТип: {tile.tileType}\nВладелец: {ownerName}\nЕда: {tile.foodAmount}\nЗолото: {tile.goldAmount}\nКамень: {tile.stoneAmount}\nДерево: {tile.woodAmount}";

            selectionPanel.SetActive(true);
        }
        else
        {
            selectionPanel.SetActive(false);
        }
    }
}