using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("SFX Pool")]
    [SerializeField] private int poolSize = 20;

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
    }
    
    public void PlayClipAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null || sfxSources.Length == 0) return;

        AudioSource source = sfxSources[nextSfxIndex];

        source.transform.position = position;
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
        mainMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}