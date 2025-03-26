using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioClip blockPlacementSound;
    public AudioClip levelClearSound;
    public AudioClip rotationSound;
    public AudioClip gameOverSound;

    private AudioSource musicSource;
    private AudioSource effectsSource;

    // Singleton pattern ´the am can be accessed from anywhere
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        // Music source setup
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = true;

        // Effects source setup
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.loop = false;

        // Start playing background music
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void PlayBlockPlacementSound()
    {
        PlaySound(blockPlacementSound);
    }

    public void PlayLevelClearSound()
    {
        PlaySound(levelClearSound);
    }

    public void PlayRotationSound()
    {
        PlaySound(rotationSound);
    }

    public void PlayGameOverSound()
    {
        musicSource.Stop();
        PlaySound(gameOverSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }
}