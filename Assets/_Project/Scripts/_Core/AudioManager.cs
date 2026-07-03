using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("SFX Pool")]
    [SerializeField] private int poolSize = 20;

    [Header("Zoom-based Volume")]
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 50f;
    [SerializeField] private Camera mainCamera;

    [Header("Music")]
    [SerializeField] private AudioClip[] musicTracks;

    private AudioSource musicSource;
    private int currentTrackIndex = -1;

    private SimulationManager simulationManager;

    private AudioSource[] sfxSources;

    private int nextSfxIndex = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);

            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        
        sfxSources = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"SFX_Source_{i}");
            
            go.transform.SetParent(transform);

            sfxSources[i] = go.AddComponent<AudioSource>();
            sfxSources[i].outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
            sfxSources[i].spatialBlend = 1f;
            sfxSources[i].minDistance = 5f;
            sfxSources[i].maxDistance = 50f;
            sfxSources[i].playOnAwake = false;
        }

        GameObject mgo = new GameObject($"Music_Source");
    
        musicSource = mgo.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Music")[0];
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        simulationManager = FindFirstObjectByType<SimulationManager>();
    }
    
    public void PlayClipAtPosition(AudioClip clip, Vector2 position)
    {
        if (clip == null) return;

        if (sfxSources == null || sfxSources.Length == 0) return;
        
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera == null) return;
        
        if (simulationManager != null && simulationManager.SimulationSpeed > 3f) return;
        
        if ((minZoom >= maxZoom || maxZoom <= 0f) && mainCamera != null)
        {
            float fallbackVolume = Mathf.Clamp01(1f - (mainCamera.orthographicSize / 100f));

            if (fallbackVolume <= 0f) return;
        }

        AudioSource source = sfxSources[nextSfxIndex];

        source.transform.position = new Vector3(position.x, position.y, 0f);
        source.PlayOneShot(clip);

        nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Length;
    }

    public void UpdateZoom(float currentZoom)
    {
        if (sfxSources == null) return;
        
        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, currentZoom);

        float effectiveMaxDist = Mathf.Lerp(80f, 20f, zoomFactor);

        foreach (var source in sfxSources)
        {
            if (source != null) source.maxDistance = effectiveMaxDist;
        }
    }
    
    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }
    
    public void PlayRandomMusic()
    {
        if (musicTracks == null || musicTracks.Length == 0) return;

        int index = Random.Range(0, musicTracks.Length);

        PlayMusic(index);
    }
    
    public void PlayMusic(int trackIndex)
    {
        if (musicTracks == null || trackIndex < 0 || trackIndex >= musicTracks.Length) return;

        if (currentTrackIndex == trackIndex && musicSource.isPlaying) return;

        currentTrackIndex = trackIndex;

        musicSource.clip = musicTracks[trackIndex];

        musicSource.Play();
    }
    
    public void StopMusic()
    {
        musicSource.Stop();

        currentTrackIndex = -1;
    }
    
    public void ToggleMusicPause()
    {
        if (musicSource.isPlaying) musicSource.Pause(); else musicSource.UnPause();
    }
}