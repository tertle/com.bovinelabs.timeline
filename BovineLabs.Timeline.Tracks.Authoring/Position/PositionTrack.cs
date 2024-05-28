// <copyright file="PositionTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using UnityEngine;
    using UnityEngine.Timeline;

    [TrackClipType(typeof(PositionStartClip))]
    [TrackClipType(typeof(PositionClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(Transform))]
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