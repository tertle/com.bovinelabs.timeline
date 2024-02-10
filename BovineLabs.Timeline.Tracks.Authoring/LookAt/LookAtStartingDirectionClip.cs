// <copyright file="LookAtStartingDirectionClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Authoring
{
    using BovineLabs.Timeline.Authoring;
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using UnityEngine.Timeline;

    public class LookAtStartingDirectionClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <inheritdoc/>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent<LookAtStartingDirection>(clipEntity);
            context.Baker.AddComponent<LookAtAnimated>(clipEntity);
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}
