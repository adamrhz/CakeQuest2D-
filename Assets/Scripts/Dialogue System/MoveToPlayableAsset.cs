using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
public enum MoveToMode
{
    ReachByClipEnd,
    ConstantSpeed
}
public enum AnimationMode
{
    Natural,
    FaceDirection,
}

public class MoveToPlayableAsset : PlayableAsset, ITimelineClipAsset
{
    public Vector3 destination;
    public MoveToMode moveMode = MoveToMode.ReachByClipEnd;
    public AnimationMode animationMode = AnimationMode.Natural;

    public Direction faceDirection = Direction.Top;
    public bool Relative = false;
    public float speed = 5f;

    public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Extrapolation;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MoveToPlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();

        behaviour.destination = destination;
        behaviour.faceDirection = faceDirection;
        behaviour.animationMode = animationMode;
        behaviour.Relative = Relative;
        behaviour.moveMode = moveMode;
        behaviour.speed = speed;

        return playable;
    }
}
