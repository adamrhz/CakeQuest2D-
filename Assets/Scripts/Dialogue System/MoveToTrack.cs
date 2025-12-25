using UnityEngine;
using UnityEngine.Timeline;

[TrackColor(0.2f, 0.7f, 1f)]
[TrackBindingType(typeof(Transform))]
[TrackClipType(typeof(MoveToPlayableAsset))]
public class MoveToTrack : TrackAsset
{
}
