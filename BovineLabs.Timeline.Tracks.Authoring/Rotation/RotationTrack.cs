// <copyright file="RotationTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using UnityEngine;
    using UnityEngine.Timeline;

    [TrackClipType(typeof(LookAtTargetClip))]
    [TrackClipType(typeof(LookAtStartingDirectionClip))]
    [TrackColor(0, 0.25f, 0)]
    [TrackBindingType(typeof(Transform))]
    public class RotationTrack : DOTSTrack
    {
        public bool ResetRotationOnDeactivate;

        protected override void Bake(BakingContext context)
        {
            if (this.ResetRotationOnDeactivate)
            {
                var trackEntity = context.CreateTrackEntity();
                context.Baker.AddComponent<RotationResetOnDeactivate>(trackEntity);
            }
        }
    }
}
