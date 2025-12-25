using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class VolumeHandler : MonoBehaviour
{
    [SerializeField] Slider contextSlider;
    [SerializeField] bool SFX;
    [SerializeField] bool Music;
    [SerializeField] bool Voice;
    [SerializeField] TMP_Text volumeText;


    private void Start()
    {
        int volume = 0;
        if (SFX)
        {
            volume = GamePreference.SFXVolume;
            contextSlider.SetValueWithoutNotify(GamePreference.SFXVolume);
            SetSFXPreference(GamePreference.SFXVolume);
        }
        else if (Music)
        {
            volume = GamePreference.MusicVolume;
            contextSlider.SetValueWithoutNotify(GamePreference.MusicVolume);
            SetMusicPreference(GamePreference.MusicVolume);

        }
        else if (Voice)
        {
            volume = GamePreference.VoiceVolume;
            contextSlider.SetValueWithoutNotify(GamePreference.VoiceVolume);
            SetVoicePreference(GamePreference.VoiceVolume);

        }

        SetVolume(volume);

    }
    public void SetVolume(float volume)
    {
        float value = volume; ;
        volumeText.SetText($"{(int)value}%");
        if (SFX)
        {
            SetSFXPreference((int)value);
        }
        else if (Music)
        {
            SetMusicPreference((int)value);

        }
        else if (Voice)
        {
            SetVoicePreference((int)value);

        }
        ApplySoundPrefs();

    }
    public static void ApplySoundPrefs()
    {
        RAudio.GSetValueParam("Music Volume", GamePreference.MusicVolume); // Call the static method directly
        RAudio.GSetValueParam("SFX Volume", GamePreference.SFXVolume); // Call the static method directly
        RAudio.GSetValueParam("Voice Volume", GamePreference.VoiceVolume); // Call the static method directly

    }


    public void SetSFXPreference(int volume)
    {
        GamePreference.SFXVolume = volume;
        ApplySoundPrefs();
    }
    public void SetMusicPreference(int volume)
    {
        GamePreference.MusicVolume = volume;
        ApplySoundPrefs();

    }
    public void SetVoicePreference(int volume)
    {
        GamePreference.VoiceVolume = volume;
        ApplySoundPrefs();

    }
}
