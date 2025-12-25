using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DialogueLoopPlayableBehaviour : PlayableBehaviour
{
    public CutsceneEventMethod pauseMethod = CutsceneEventMethod.Pause;
    bool firstFrame = true;
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if(Timeline.CurrentlyPlayingTimeline == null)
        {
            return;
        }
        if (firstFrame)
        {
            switch (pauseMethod)
            {
                case CutsceneEventMethod.Loop:
                    var start = 0;
                    var end = start + playable.GetDuration();
                    Timeline.CurrentlyPlayingTimeline?.StartDialogueLoop((float)start, (float)end, withOffset: false);
                    break;
                case CutsceneEventMethod.Pause:
                    Timeline.CurrentlyPlayingTimeline?.StartDialoguePause();

                    break;
                case CutsceneEventMethod.CamShake:
                    float startTime = 0;
                    float endTime = startTime + (float)playable.GetDuration();
                    Timeline.CurrentlyPlayingTimeline?.StartCamShake((float)startTime, (float)endTime);
                    CamManager.Shake((float)playable.GetDuration(), .15f);
                    break;
                case CutsceneEventMethod.Flash:
                    float flashTime = (float)playable.GetDuration() / 3f;
                    FadeScreen.SetColor(Color.white);
                    FadeScreen.Flash(flashTime, flashTime);
                    break;
                default:
                    break;
            }
            firstFrame = false;
        }
    }
}