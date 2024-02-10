// <copyright file="LookAtTargetTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using UnityEngine;
    using UnityEngine.Timeline;

    [TrackClipType(typeof(LookAtTargetClip))]
    [TrackClipType(typeof(LookAtStartingDirectionClip))]
    [TrackColor(0, 0.25f, 0)]
    [TrackBindingType(typeof(Transform))]
    public class RotationTrack : DOTSTrack
    {
    }
}
