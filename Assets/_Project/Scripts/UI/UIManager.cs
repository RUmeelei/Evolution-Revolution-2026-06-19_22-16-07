using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private Button mainMenuButton;

    [Header("Tutorial Panel")]
    [SerializeField] private GameObject tutorialPanel;

    private bool tutorialPassed = false;

    private FactionManager factionManager;

    private SimulationManager simulationManager;

    void Awake()
    {
        GameManager.RegisterUIManager(this);
    }

    void Start()
    {
        factionManager = GameManager.FactionManager;

        simulationManager = GameManager.SimulationManager;

        WinConditionManager wcm = GameManager.WinConditionManager;

        if (wcm != null) wcm.OnVictory += ShowVictoryScreen;
    }

    void Update()
    {
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
}