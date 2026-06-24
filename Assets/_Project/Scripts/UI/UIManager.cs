using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI foodValue;
    [SerializeField] private Image foodIcon;

    [Header("Time Controls")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button speed1xButton;
    [SerializeField] private Button speed2xButton;

    [Header("Selection Info")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI selectionInfoText;

    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private Button mainMenuButton;

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
        
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPause);

        if (playButton != null) playButton.onClick.AddListener(OnPlay);

        if (speed1xButton != null) speed1xButton.onClick.AddListener(OnSpeed1x);

        if (speed2xButton != null) speed2xButton.onClick.AddListener(OnSpeed2x);

        WinConditionManager wcm = FindFirstObjectByType<WinConditionManager>();

        if (wcm != null) wcm.OnVictory += ShowVictoryScreen;
    }

    void Update()
    {
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
        if (simulationManager != null) simulationManager.PauseSimulation();
    }

    void OnPlay()
    {
        if (simulationManager != null) simulationManager.ResumeSimulation();
    }

    void OnSpeed1x()
    {
        if (simulationManager != null) simulationManager.SetSimulationSpeed(1);
    }

    void OnSpeed2x()
    {
        if (simulationManager != null) simulationManager.SetSimulationSpeed(2);
    }
}