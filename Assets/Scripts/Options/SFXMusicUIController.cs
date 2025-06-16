using UnityEngine;
using UnityEngine.UI;
public class SFXMusicUIController : MonoBehaviour
{
    public Slider sfxSlider, musicSlider;
    private void Start() {
        // Load saved preferences at start
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);    // Default to 1 if not set
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        SFXManager.Instance.SFXVolume(sfxSlider.value);
        SFXManager.Instance.MusicVolume(musicSlider.value);
    }
    public void MusicVolume()
    {
        float vol = musicSlider.value;
        SFXManager.Instance.MusicVolume(vol);
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }
    public void SfxVolume()
    {
        float vol = sfxSlider.value;
        SFXManager.Instance.SFXVolume(vol);
        PlayerPrefs.SetFloat("SFXVolume", vol);
    }
}