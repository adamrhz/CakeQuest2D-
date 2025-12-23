using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public class Timeline : MonoBehaviour
{

    public static bool IsInCutscene = false;
    public static Timeline CurrentlyPlayingTimeline;
    Tuple<float, float> loop;
    Tuple<float, float> shakingTimeStamp;

    public ConditionResultObject[] condition;
    public Cutscene storagePlay;
    public bool started = false;
    public PlayableDirector playableDirector;
    public UnityEvent OnCutsceneOver;
    public bool Automatic;




    protected void Start()
    {
        if (Automatic)
        {
            if (FadeScreen.fading || FadeScreen.fadeOn)
            {

                StartCinematic(true);
            }
            else
            {
                StartCinematic();
            }
           
        }
    }

    public void StartDialoguePause()
    {
        Pause();
        StartDialogue();
    }

    public virtual void StartCinematic(bool delayed = false)
    {
        if (CanPlayCutscene())
        {
            // Debug.Log("Playing Cutscene");
            storagePlay.ResetPlayed();
            Character.Player?.ToggleCutsceneState();
            SetupRequirements();
            if (delayed)
            {
                StartLoop(0f, 0.01f);
                FadeScreen.AddOnEndFadeEvent(EndLoop);
            }
            playableDirector.Play();
            IsInCutscene = true;
            CurrentlyPlayingTimeline = this;
        }

    }

    public void StartCamShake(float start, float end, bool withOffset = true)
    {
        start = withOffset ? start : start + (float)playableDirector.time;
        end = withOffset ? end : end + (float)playableDirector.time;
        shakingTimeStamp = new Tuple<float, float>(start, end);
        CamManager.Shake(-1, .15f);
    }

    public void StartDialogueLoop(float start, float end, bool withOffset = true)
    {
        StartLoop(start, end, withOffset);
        StartDialogue();
    }

    public void StartLoop(float start, float end, bool withOffset = true)
    {
        start = withOffset ? start : start + (float)playableDirector.time;
        end = withOffset ? end : end + (float)playableDirector.time;
        loop = new Tuple<float, float>(start, end);
    }

    public static void SkipCurrentCutscene()
    {
        CurrentlyPlayingTimeline.SkipCutscene();
    }

    private void SkipCutscene()
    {
        storagePlay.MakeUnPlayable();
        UICanvas.CancelCurrentDialogue();
        playableDirector.time = playableDirector.playableAsset.duration - .001f;
        EndLoop();
        EndPause();
    }

    public virtual void SetupRequirements()
    {
        playableDirector.playableAsset = storagePlay.CutsceneToPlay;
    }


    public void PlaySong(string songName)
    {

        MusicPlayer.Singleton.PlaySong(songName);
    }

    public virtual void StartDialogue()
    {
        //Debug.Log("Starting Dialogue");
        if (!started)
        {
            if (CanPlayCutscene())
            {
                if (storagePlay.GetCurrentLine() != null)
                {

                    started = true;

                    //playableDirector.Pause();
                    DialogueRequest();
                    return;
                }
            }
                DialogueOver();
        }
    }

    private void Update()
    {
                if (loop != null && playableDirector.time > loop.Item2)
                {
                    playableDirector.time = loop.Item1;
                    playableDirector.Evaluate();
                    playableDirector.Play();
                }

                if(shakingTimeStamp != null && playableDirector.time > shakingTimeStamp.Item2)
                {
                    CamManager.StopShake();
                    shakingTimeStamp = null;
                }
    }




    public void Pause()
    {
        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
    }


    public void SetCutscene(Cutscene cutscene)
    {
        storagePlay = cutscene;
    }
    public virtual void DialogueRequest()
    {
        Dialogue dialogue = storagePlay.GetNextLine();
        dialogue.OnOverEvent.AddListener(DialogueOver);
        //Debug.Log("Requesting Dialogue : " + dialogue.OnOverEvent.GetNonPersistentEventCount());
        UICanvas.StartDialogue(dialogue, null, null);
    }
    public void EndLoop()
    {
        loop = null;
    }

    public virtual void DialogueOver()
    {
        if (!IsInCutscene)
        {
            return;
        }

        started = false;
        EndLoop();
        EndPause();
        //Debug.Log("Dialogue Over");
        // UnpauseCutscene();
    }

    private void EndPause()
    {
        if(playableDirector.playableGraph.GetRootPlayable(0).GetSpeed() == 0)
        {
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        }
    }

    public void UnpauseCutscene()
    {
        playableDirector?.Resume();
       // Debug.Log("Unpause");
    }

    public virtual void CutsceneOver()
    {
        storagePlay.ResetPlayed();
        if (storagePlay)
        {
            if (!storagePlay.repeats)
            {

                storagePlay.RuntimeValue = true;
            }
        }
        CurrentlyPlayingTimeline = null;
        IsInCutscene = false;
    }
    public void StopReplay()
    {
        if (storagePlay)
        {
            if (!storagePlay.repeats)
            {

                storagePlay.RuntimeValue = true;
            }
        }
    }

    public bool CheckCondition()
    {
        foreach (ConditionResultObject c in condition)
        {
            if (!c.CheckCondition())
            {
                return false;
            }
        }
        return true;
    }
    public bool CanPlayCutscene()
    {
        if (!CheckCondition())
        {
            return false;
        }
        if (storagePlay)
        {
            return !storagePlay.RuntimeValue;
        }
        return false;
    }

    public void FadeTo()
    {
        FadeScreen.SetColor(Color.white);
        StartCoroutine(FadeScreen.Singleton.StartFadeAnimation(true, .1f));
    }


    public void Flash()
    {
        FadeScreen.SetColor(Color.white);
        StartCoroutine(FadeScreen.Singleton.StartFlashAnimation(.1f));
    }
    public void PlayCutscene()
    {
        playableDirector.Play();
    }
    public void DeleteCutscene()
    {
        Destroy(gameObject);
    }
}
