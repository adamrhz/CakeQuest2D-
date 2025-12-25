using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{

    public static SFXPlayer instance;

    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource voiceSource;
    [SerializeField] AudioSource musicSource;

    [SerializeField] AudioClip[] navigate;
    [SerializeField] AudioClip[] select;
    [SerializeField] AudioClip[] cancel;
    [SerializeField] AudioClip[] voiceClips;
    [SerializeField] AudioClip[] sfxs;
    [SerializeField] AudioClip[] melodies;


    private void Awake()
    {
        instance = this;
    }
    private void PlayRandomClip(AudioSource source, AudioClip[] clips, bool canCancel = true)
    {

        return;
        if (clips != null && clips.Length > 0)
        {
            AudioClip clip = clips[Random.Range(0, clips.Length)];
            if (!source.isPlaying || canCancel)
            {
                source.PlayOneShot(clip);

            }
        }
    }

    public void PlayOnNavigate()
    {
        RAudio.Play("UI_Navigate");
        PlayRandomClip(sfxSource, navigate);
    }

    public void PlayOnSelect()
    {
        RAudio.Play("UI_Confirm");
        PlayRandomClip(sfxSource, select);
    }

    public void PlayOnBack()
    {
        RAudio.Play("UI_Return");
        PlayRandomClip(sfxSource, cancel);
    }

    public void PlayOnVolumeChange()
    {
        PlayRandomClip(sfxSource, sfxs, false);
    }

    public void PlayOnMusicVolumeChange()
    {
        PlayRandomClip(musicSource, melodies, false);
    }

    public void PlayOnVoiceVolumeChange()
    {
        PlayRandomClip(voiceSource, voiceClips, false);
    }
}
