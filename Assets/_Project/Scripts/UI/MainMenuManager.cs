using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
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
        Debug.Log("Настройки пока не реализованы.");
    }

    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}