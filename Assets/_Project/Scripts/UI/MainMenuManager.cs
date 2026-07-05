using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SettingsUI settingsUI;
    
    public void NewGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LoadGame()
    {
        Debug.Log("Загрузка игры пока не реализована.");
    }

    public void OpenSettings()
    {
        settingsUI?.OpenSettings();
    }

    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}