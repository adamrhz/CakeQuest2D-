using UnityEngine;
using UnityEngine.Playables;

public class MoveToPlayableBehaviour : PlayableBehaviour
{
    public Transform target;
    public Vector3 destination;
    public MoveToMode moveMode;
    public AnimationMode animationMode;
    public Direction faceDirection;
    public bool Relative;
    public float speed;

    private Vector3 startPosition;
    private bool initialized;



    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (Timeline.CurrentlyPlayingTimeline == null)
        {
            return;
        }


        if (!initialized)
        {
            target = playerData as Transform;
            if (target == null) return;

            startPosition = target.position;
            if (Relative) { destination += startPosition; }
            if (animationMode == AnimationMode.FaceDirection)
            {
                target.gameObject.GetComponent<AnimationController>().LookAt(RoomMove.DirectionToVector(faceDirection));
            }
            initialized = true;
        }


        if (!initialized || target == null)
            return;

        double time = playable.GetTime();
        double duration = playable.GetDuration();

        Vector3 newPos;

        if (moveMode == MoveToMode.ReachByClipEnd && duration > 0)
        {
            float t = Mathf.Clamp01((float)(time / duration));
            newPos = Vector3.Lerp(startPosition, destination, t);
        }
        else // ConstantSpeed
        {
            newPos = Vector3.MoveTowards(target.position, destination, speed * Time.deltaTime);
        }
        if (animationMode == AnimationMode.Natural)
        {
            target?.gameObject?.GetComponent<Character>()?.CutsceneMoving(target.position, newPos);
        }
        else
        {
            if (target.position != newPos)
            {
                target?.gameObject?.GetComponent<Character>()?.CutsceneMoving(RoomMove.DirectionToVector(faceDirection) * 0.5f, RoomMove.DirectionToVector(faceDirection) * 0.5f);
            }
        }

        target.position = newPos;
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (Timeline.CurrentlyPlayingTimeline == null)
        {
            return;
        }
        target?.gameObject?.GetComponent<Character>()?.CutsceneMoving(Vector3.zero, Vector3.zero);
        initialized = false;
    }
}
