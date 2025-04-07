using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Variables
    public static AudioManager sharedInstanceAudioManager;

    [Header("----------- Audio Source -----------")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource ambientSource;
    [Header("----------- Audio Clip -----------")]
    public AudioClip mainMenuMusic;

    #endregion
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (sharedInstanceAudioManager == null)
        {
            sharedInstanceAudioManager = this;
        }
    }
    #region Play Clips
    public void PlayClipSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    public void PlayClipMusic(AudioClip clip)
    {
        musicSource.PlayOneShot(clip);
    }
    public void PlayClipAmbient(AudioClip clip)
    {
        ambientSource.PlayOneShot(clip);
    }
    public void PlayMusicMainMenu()
    {
        PlayClipMusic(mainMenuMusic);
    }
    #endregion
    #region Stop sounds
    public void StopMusic()
    {
        musicSource.Stop();
    }
    public void StopAmbient()
    {
        ambientSource.Stop();
    }
    public void StopSFX()
    {
        sfxSource.Stop();
    }
    public void StopAllSound()
    {
        StopAmbient();
        StopMusic();
        StopSFX();
    }
    #endregion

}
