// <copyright file="PositionTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using UnityEngine;
    using UnityEngine.Timeline;

    [TrackClipType(typeof(MoveToStartingPositionClip))]
    [TrackClipType(typeof(MovePositionClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(Transform))]
    public class PositionTrack : DOTSTrack
    {
    }
}
