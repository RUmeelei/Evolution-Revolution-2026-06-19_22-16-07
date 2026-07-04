using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DiplomacyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject diplomacyPanel;
    [SerializeField] private TextMeshProUGUI diplomacyStatusText;
    [SerializeField] private Button declareWarButton;
    [SerializeField] private Button offerPeaceButton;
    [SerializeField] private Button closeDiplomacyButton;

    private DiplomacyManager diplomacyManager;

    private FactionManager factionManager;

    private SelectionManager selectionManager;

    private TileManager tileManager;

    private int playerFaction = -1;

    private int selectedDiplomacyFaction = 0;

    void Start()
    {
        diplomacyManager = GameManager.DiplomacyManager;

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

        if (declareWarButton != null) declareWarButton.onClick.AddListener(OnDeclareWar);

        if (offerPeaceButton != null) offerPeaceButton.onClick.AddListener(OnOfferPeace);

        if (closeDiplomacyButton != null) closeDiplomacyButton.onClick.AddListener(CloseDiplomacyPanel);

        diplomacyPanel.SetActive(false);
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.V) && diplomacyPanel != null)
        // {
        //     diplomacyPanel.SetActive(!diplomacyPanel.activeSelf);
        // }

        if (!diplomacyPanel.activeSelf) return;
        
        UpdateDiplomacyInfo();
    }

    private void UpdateDiplomacyInfo()
    {
        if (diplomacyManager == null || factionManager == null || selectionManager == null || tileManager == null) return;

        int playerFactionId = GetPlayerFaction();

        Vector2Int? selectedTile = selectionManager.GetLastClickedTile();

        int selectedFactionId = -1;

        if (selectedTile.HasValue)
        {
            TileData tile = tileManager.GetTile(selectedTile.Value.x, selectedTile.Value.y);

            selectedFactionId = tile.factionId;
        }

        selectedDiplomacyFaction = selectedFactionId;

        FactionData selectedFaction = factionManager.GetFaction(selectedFactionId);

        string selectedFactionName = (selectedFactionId != -1 && selectedFaction != null) ? selectedFaction.factionName : "Нейтральные";

        var relations = (selectedFactionId != -1) ? diplomacyManager.GetRelations(playerFactionId, selectedFactionId) : null;

        string status = relations != null ? (relations.atWar ? "Война" : "Мир") : "Нейтралитет";

        float tension = relations != null ? relations.tension : 0f;

        diplomacyStatusText.text = $"Фракция: {selectedFactionName}\nСтатус: {status}\nНапряжение: {tension:F0}%";
    }

    private void OnDeclareWar()
    {
        if (diplomacyManager == null || selectedDiplomacyFaction == -1) return;

        int playerFactionId = GetPlayerFaction();

        diplomacyManager.DeclareWar(playerFactionId, selectedDiplomacyFaction);

        UpdateDiplomacyInfo();
    }

    private void OnOfferPeace()
    {
        if (diplomacyManager == null || selectedDiplomacyFaction == -1) return;

        int playerFactionId = GetPlayerFaction();

        diplomacyManager.MakePeace(playerFactionId, selectedDiplomacyFaction);

        UpdateDiplomacyInfo();
    }

    public void OpenDiplomacyPanel()
    {
        if (diplomacyPanel != null) diplomacyPanel.SetActive(true);

        UpdateDiplomacyInfo();
    }

    public void CloseDiplomacyPanel()
    {
        if (diplomacyPanel != null) diplomacyPanel.SetActive(false);

        UpdateDiplomacyInfo();
    }

    private int GetPlayerFaction()
    {
        for (int i = 0; i < factionManager.FactionCount; i++)
        {
            if (factionManager.IsPlayerFaction(i)) return i;
        }

        return 0;
    }
}