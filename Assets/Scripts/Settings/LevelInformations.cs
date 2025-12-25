using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInformations : MonoBehaviour
{
    public AudioClip levelMainTheme;
    public EventReference levelEventReference;
    public Color CameraBackgroundColor;

    public void Start()
    {
        MusicPlayer.Singleton?.PlaySong(levelEventReference);
        Camera.main.backgroundColor = CameraBackgroundColor;


    }
}
