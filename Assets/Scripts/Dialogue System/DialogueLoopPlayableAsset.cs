using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class DialogueLoopPlayableAsset : PlayableAsset
{
    public CutsceneEventMethod pauseMethod = CutsceneEventMethod.Loop;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueLoopPlayableBehaviour>.Create(graph);
        playable.GetBehaviour().pauseMethod = pauseMethod;
        return playable;
    }
}


[System.Serializable]
public enum CutsceneEventMethod
{
    Loop,
    Pause,
    CamShake,
    Flash,
}
