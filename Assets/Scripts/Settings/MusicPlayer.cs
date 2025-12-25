using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{

    private bool _transitioning = false;
    public float volumeTransitionSpeed = 4f; // Speed at which volume changes
    private static MusicPlayer _singleton;
    public static MusicPlayer Singleton
    {
        get
        {
            if (_singleton == null)
            {
                // Load the MusicPlayer prefab from Resources
                GameObject musicPlayerPrefab = Resources.Load<GameObject>("MusicPlayer");
                if (musicPlayerPrefab != null)
                {
                    GameObject musicPlayerInstance = Instantiate(musicPlayerPrefab);
                    Singleton = musicPlayerInstance.GetComponent<MusicPlayer>();
                    //Debug.Log("MusicPlayer Instantiated");
                }
                else
                {
                    Debug.LogError("MusicPlayer prefab not found in Resources.");
                }
            }
            return _singleton;
        }
        private set
        {
            if (_singleton == null)
            {
                _singleton = value;
            }
            else if (_singleton != value)
            {
                Debug.LogWarning($"{nameof(MusicPlayer)} instance already exists. Destroying duplicate!");
                Destroy(value.gameObject);
            }
        }
    }
    void Awake()
    {
        if (_singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (_singleton != this)
        {
            Destroy(this.gameObject);
        }
    }


    public void PlaySong(string songName, bool loops = false)
    {

        if (!string.IsNullOrEmpty(songName))
        {
            // Load the sprite from Resources folder
            string fullPath = "Soundtrack/" + songName; // Assuming the path is relative to the Resources folder

            AudioClip Song = Resources.Load(fullPath) as AudioClip;
            if (Song != null)
            {
                //PlaySong(Song, loops);
            }
        }
    }
    public static void Stop()
    {
        RAudio.PauseSong();
    }
    public static void Resume()
    {
        RAudio.ResumeSong();
    }
    public void PlaySong(EventReference track)
    {
        StartCoroutine(TransitioningSong(track));
    }
    private IEnumerator TransitioningSong(EventReference _track)
    {
        while (_transitioning)
        {
            yield return null;
        }
        _transitioning = true;
        RAudio.StopSong();
        yield return .5f;
        RAudio.PlaySong(_track);
        _transitioning = false;
    }

}
