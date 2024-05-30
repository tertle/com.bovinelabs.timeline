// <copyright file="SubDirectorTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System.ComponentModel;
    using UnityEngine.Timeline;

    [System.Serializable]
    [TrackColor(0.5f, 0.1f, 0.5f)]
    [TrackClipType(typeof(SubDirectorClip))]
    [TrackClipType(typeof(SubTimelineClip))]
    [DisplayName("DOTS/Sub Director Track")]
    public class SubDirectorTrack : DOTSTrack
    {
    }
}