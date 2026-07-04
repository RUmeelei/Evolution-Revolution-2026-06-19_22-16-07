using UnityEngine;
using TMPro;

public class PoliticsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject politicsPanel;
    [SerializeField] private TextMeshProUGUI politicsText;

    private PoliticsManager politicsManager;

    private FactionManager factionManager;

    private int playerFaction = -1;

    void Start()
    {
        politicsManager = GameManager.PoliticsManager;

        factionManager = GameManager.FactionManager;

        if (factionManager != null)
        {
            for (int i = 0; i < factionManager.FactionCount; i++)
            {
                if (factionManager.IsPlayerFaction(i))
                {
                    playerFaction = i; break;
                }
            }
        }
    }

    void Update()
    {
        if (politicsManager == null || factionManager == null || politicsPanel == null || politicsText == null) return;

        if (playerFaction == -1) return;

        var groups = politicsManager.GetFactionGroups(playerFaction);
        
        if (groups == null)
        {
            politicsPanel.SetActive(false); return;
        }

        string display = "";
        string[] groupNames = { "Элита", "Произв.", "Торговцы", "Идеологи", "Военные" };

        for (int g = 0; g < groups.Length; g++)
        {
            display += $"{groupNames[g]}: {groups[g].loyalty:F0}%\n";
        }

        politicsText.text = display.TrimEnd('\n');
        
        politicsPanel.SetActive(true);
    }
}