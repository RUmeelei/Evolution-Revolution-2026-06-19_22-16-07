using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button closeButton;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = GameManager.AudioManager;

        if (audioManager == null)
        {
            Debug.LogError("AudioManager не найден в GameManager!"); return;
        }
        
        LoadVolumes();
        
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
            
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void LoadVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.2f);

        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
        if (musicVolumeSlider != null) musicVolumeSlider.value = music;

        audioManager?.SetMasterVolume(master);
        audioManager?.SetSFXVolume(sfx);
        audioManager?.SetMusicVolume(music);
    }

    private void OnMasterVolumeChanged(float value)
    {
        audioManager?.SetMasterVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        audioManager?.SetSFXVolume(value);
    }

    private void OnMusicVolumeChanged(float value)
    {
        audioManager?.SetMusicVolume(value);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            LoadVolumes();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }
}