using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI foodValue;
    [SerializeField] private Image foodIcon;

    [Header("Time Controls")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image pauseImage;
    [SerializeField] private Button speedIncreaseButton;
    [SerializeField] private Button speedDecreaseButton;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Image[] speedBars;
    [SerializeField] private Color activeSpeedColor = Color.green;
    [SerializeField] private Color inactiveSpeedColor = Color.gray;

    [Header("Selection Info")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI selectionInfoText;

    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private Button mainMenuButton;

    [Header("Politics Panel")]
    [SerializeField] private GameObject politicsPanel;
    [SerializeField] private TextMeshProUGUI politicsText;

    [Header("Diplomacy Panel")]
    [SerializeField] private GameObject diplomacyPanel;
    [SerializeField] private TextMeshProUGUI diplomacyStatusText;
    [SerializeField] private Button declareWarButton;
    [SerializeField] private Button offerPeaceButton;
    [SerializeField] private Button closeDiplomacyButton;

    [Header("Tutorial Panel")]
    [SerializeField] private GameObject tutorialPanel;

    private bool tutorialPassed = false;

    private int selectedDiplomacyFaction = 0;

    private DiplomacyManager diplomacyManager;
    private PoliticsManager politicsManager;
    private SelectionManager selectionManager;
    private UnitManager unitManager;
    private EconomyManager economyManager;
    private FactionManager factionManager;
    private SimulationManager simulationManager;
    private TileManager tileManager;

    void Start()
    {
        economyManager = FindFirstObjectByType<EconomyManager>();

        factionManager = FindFirstObjectByType<FactionManager>();

        simulationManager = FindFirstObjectByType<SimulationManager>();

        tileManager = FindFirstObjectByType<TileManager>();

        selectionManager = FindFirstObjectByType<SelectionManager>();

        unitManager = FindFirstObjectByType<UnitManager>();
        
        politicsManager = FindFirstObjectByType<PoliticsManager>();

        diplomacyManager = FindFirstObjectByType<DiplomacyManager>();

        if (declareWarButton != null) declareWarButton.onClick.AddListener(OnDeclareWar);

        if (offerPeaceButton != null) offerPeaceButton.onClick.AddListener(OnOfferPeace);

        if (closeDiplomacyButton != null) closeDiplomacyButton.onClick.AddListener(OnCloseDiplomacy);
        
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPause);

        if (speedIncreaseButton != null) speedIncreaseButton.onClick.AddListener(OnSpeedIncrease);

        if (speedDecreaseButton != null) speedDecreaseButton.onClick.AddListener(OnSpeedDecrease);

        WinConditionManager wcm = FindFirstObjectByType<WinConditionManager>();

        if (wcm != null) wcm.OnVictory += ShowVictoryScreen;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V) && diplomacyPanel != null)
        {
            diplomacyPanel.SetActive(!diplomacyPanel.activeSelf);
        }
        
        if (pauseImage != null && simulationManager != null) pauseImage.sprite = simulationManager.Running ? playIcon : pauseIcon;
            
        if (speedBars != null && simulationManager != null)
        {
            int currentSpeed = simulationManager.SimulationSpeed;

            for (int i = 0; i < speedBars.Length; i++)
            {
                if (speedBars[i] != null) speedBars[i].color = (i < currentSpeed) ? activeSpeedColor : inactiveSpeedColor;
            }
        }

        if (economyManager != null && factionManager != null && foodValue != null)
        {
            for (int i = 0; i < factionManager.FactionCount; i++)
            {
                if (factionManager.IsPlayerFaction(i))
                {
                    foodValue.text = economyManager.GetFood(i).ToString("F0");

                    break;
                }
            }
        }
        
        if (politicsManager != null && factionManager != null && politicsText != null)
        {
            for (int i = 0; i < factionManager.FactionCount; i++)
            {
                if (factionManager.IsPlayerFaction(i))
                {
                    var groups = politicsManager.GetFactionGroups(i);

                    if (groups != null)
                    {
                        string display = "";
                        string[] groupNames = { "Элита", "Произв.", "Торговцы", "Идеологи", "Военные" };

                        for (int g = 0; g < groups.Length; g++)
                        {
                            display += $"{groupNames[g]}: {groups[g].loyalty:F0}%\n";
                        }
                        politicsText.text = display.TrimEnd('\n');
                        politicsPanel.SetActive(true);
                    }
                    else
                    {
                        politicsText.text = "";
                        politicsPanel.SetActive(false);
                    }

                    break;
                }
            }
        }
        
        if (diplomacyPanel != null && diplomacyPanel.activeSelf && diplomacyManager != null && factionManager != null)
        {
            int playerFaction = GetPlayerFaction();

            Vector2Int? selectedTile = selectionManager.GetLastClickedTile();

            int selectedId = (selectedTile != null && tileManager.GetTile(selectedTile.Value.x, selectedTile.Value.y).factionId != -1) ? tileManager.GetTile(selectedTile.Value.x, selectedTile.Value.y).factionId : -1;

            selectedDiplomacyFaction = selectedId;

            FactionData selectedFaction = factionManager.GetFaction(selectedId);

            string selectedFactionName = (selectedTile != null && selectedId != -1) ? selectedFaction.factionName : "Нейтральные";

            var rel = (selectedTile != null && selectedId != -1) ? diplomacyManager.GetRelations(playerFaction, selectedDiplomacyFaction) : null;

            string status = rel != null ? rel.atWar ? "Война" : "Мир" : "Нейтралитет";

            if (selectedDiplomacyFaction != -1)
            {
                diplomacyStatusText.text = $"Фракция: {selectedFactionName}\nСтатус: {status}\nНапряжение: {rel.tension:F0}%";
            }
            else
            {
                diplomacyStatusText.text = $"Фракция: {selectedFactionName}\nСтатус: {status}\nНапряжение: 0%";
            }
        }
        
        if (selectionInfoText != null && selectionManager != null && unitManager != null && tileManager != null)
        {
            int selectedUnitIndex = selectionManager.GetFirstSelectedUnit();

            Vector2Int? selectedTile = selectionManager.GetLastClickedTile();

            if (selectedUnitIndex >= 0)
            {
                HumanData unit = unitManager.Humans[selectedUnitIndex];

                string factionName = factionManager.GetFaction(unit.factionId)?.factionName ?? "Неизвестно";

                selectionInfoText.text = $"Юнит\nЗдоровье: {unit.hp:F0}/{unit.maxHp}\nСтамина: {unit.stamina:F0}/{unit.maxStamina}\nФракция: {factionName}";

                selectionPanel.SetActive(true);
            }
            else if (selectedTile.HasValue)
            {
                TileData tile = tileManager.GetTile(selectedTile.Value.x, selectedTile.Value.y);

                string ownerName = "Нейтральный";

                if (tile.factionId >= 0) ownerName = factionManager.GetFaction(tile.factionId)?.factionName ?? "Неизвестно"; selectionInfoText.text = $"Тайл ({selectedTile.Value.x},{selectedTile.Value.y})\nТип: {tile.tileType}\nВладелец: {ownerName}\nЕда: {tile.foodAmount}";

                selectionPanel.SetActive(true);
            }
            else
            {
                selectionInfoText.text = "";

                selectionPanel.SetActive(false);
            }
        }

        if (simulationManager.Running && !tutorialPassed)
        {
            tutorialPanel.SetActive(false);
            
            tutorialPassed = true;
        }
    }

    private void ShowVictoryScreen(int winnerFactionId)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryText != null)
            {
                string factionName = factionManager.GetFaction(winnerFactionId)?.factionName ?? "Неизвестная фракция";
                string factionLeader = factionManager.GetFaction(winnerFactionId)?.rulerName ?? "Неизвестный лидер";

                victoryText.text = $"Фракция ``{factionName}`` одержала победу!\nЛидер фракции - {factionLeader}";
            }
        }
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    void OnPause()
    {
        if (simulationManager != null) if (simulationManager.Running) simulationManager.PauseSimulation(); else simulationManager.ResumeSimulation();
    }

    void OnSpeedIncrease()
    {
        if (simulationManager != null) simulationManager.IncreaseSimulationSpeed();
    }

    void OnSpeedDecrease()
    {
        if (simulationManager != null) simulationManager.DecreaseSimulationSpeed();
    }

    void OnDeclareWar()
    {
        if (diplomacyManager == null || factionManager == null || selectedDiplomacyFaction == -1) return;

        int playerFaction = GetPlayerFaction();

        diplomacyManager.DeclareWar(playerFaction, selectedDiplomacyFaction);
    }

    void OnOfferPeace()
    {
        if (diplomacyManager == null || factionManager == null || selectedDiplomacyFaction == -1) return;

        int playerFaction = GetPlayerFaction();

        diplomacyManager.MakePeace(playerFaction, selectedDiplomacyFaction);
    }

    void OnCloseDiplomacy()
    {
        if (diplomacyPanel != null) diplomacyPanel.SetActive(false);
    }

    int GetPlayerFaction()
    {
        for (int i = 0; i < factionManager.FactionCount; i++)
        {
            if (factionManager.IsPlayerFaction(i)) return i;
        }

        return 0;
    }
}