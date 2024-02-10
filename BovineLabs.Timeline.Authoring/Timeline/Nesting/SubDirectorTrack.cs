// <copyright file="SubDirectorTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring.Nesting
{
    using UnityEngine.Timeline;

    [System.Serializable]
    [TrackColor(0.5f, 0.1f, 0.5f)]
    [TrackClipType(typeof(SubDirectorClip))]
    [TrackClipType(typeof(SubTimelineClip))]
    // [TrackClipType(typeof(SubPrefabClip))]
    public class SubDirectorTrack : DOTSTrack
    {
    }
}
