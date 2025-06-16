using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Load saved volume settings
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Sound[] music;
    public Sound[] SFX;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public void PlayMusic(string musicName)
    {
        Sound s = Array.Find(music, x => x.audioName == musicName);
        if (s == null)
            Debug.Log("Music Clip is not found");
        else
        {
            musicSource.clip = s.audioClip;
            musicSource.Play();
        }
    }
    public void PlaySFX(string sfxName)
    {
        Sound s = Array.Find(SFX, x => x.audioName == sfxName);
        if (s == null)
            Debug.Log("SFX Clip is not found");
        else
        {
            sfxSource.PlayOneShot(s.audioClip);
        }
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}