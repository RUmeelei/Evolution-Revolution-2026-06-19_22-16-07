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
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float minZoomVolume = 1f;
    [SerializeField] private float maxZoomVolume = 0f;
    [SerializeField] private Camera mainCamera;

    private float baseSFXVolume = 1f;

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

        GameManager.RegisterAudioManager(this);
        
        sfxSources = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = new GameObject($"SFX_Source_{i}");
            
            go.transform.SetParent(transform);

            sfxSources[i] = go.AddComponent<AudioSource>();
            sfxSources[i].outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
            sfxSources[i].spatialBlend = 1f;
            sfxSources[i].rolloffMode = AudioRolloffMode.Logarithmic;
            sfxSources[i].minDistance = 5f;
            sfxSources[i].maxDistance = 150f;
            sfxSources[i].playOnAwake = false;
        }

        GameObject mgo = new GameObject("Music_Source");

        mgo.transform.SetParent(transform);
    
        musicSource = mgo.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Music")[0];
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        simulationManager = GameManager.SimulationManager;
    }

    void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        float zoomFactor = Mathf.InverseLerp(minZoom, maxZoom, mainCamera.orthographicSize);
        float targetVolume = baseSFXVolume * Mathf.Lerp(minZoomVolume, maxZoomVolume, zoomFactor);

        mainMixer.SetFloat("SFXVolume", Mathf.Log10(targetVolume) * 100);

        if (!musicSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }
    
    public void PlayClipAtPosition(AudioClip clip, Vector2 position)
    {
        if (clip == null) return;

        if (sfxSources == null || sfxSources.Length == 0) return;
        
        if (mainCamera == null) mainCamera = Camera.main;

        if (mainCamera == null) return;
        
        if (simulationManager != null && simulationManager.SimulationSpeed > 3f) return;

        AudioSource source = sfxSources[nextSfxIndex];

        source.transform.position = new Vector3(position.x, position.y, 0f);
        source.PlayOneShot(clip);

        nextSfxIndex = (nextSfxIndex + 1) % sfxSources.Length;
    }
    
    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        baseSFXVolume = value;

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