using UnityEngine;

public class InGameMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private SettingsUI settingsUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && mainMenuPanel != null)
        {
            if (!mainMenuPanel.activeSelf) OpenMainMenu(); else {CloseMainMenu(); settingsUI?.CloseSettings();}
        }
    }

    public void CloseMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsUI != null) settingsUI.CloseSettings();
    }

    public void OpenMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        settingsUI?.OpenSettings();
    }

    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}