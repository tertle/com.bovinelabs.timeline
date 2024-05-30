// <copyright file="PositionTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using System;
    using System.ComponentModel;
    using BovineLabs.Timeline.Tracks.Data;
    using UnityEngine;
    using UnityEngine.Timeline;

    [Serializable]
    [TrackClipType(typeof(PositionStartClip))]
    [TrackClipType(typeof(PositionClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(Transform))]
    [DisplayName("DOTS/Transform Position Track")]
    public class TransformPositionTrack : DOTSTrack
    {
        public bool ResetPositionOnDeactivate;

        protected override void Bake(BakingContext context)
        {
            if (this.ResetPositionOnDeactivate)
            {
                var trackEntity = context.CreateTrackEntity();
                context.Baker.AddComponent<PositionResetOnDeactivate>(trackEntity);
            }
        }
    }
}