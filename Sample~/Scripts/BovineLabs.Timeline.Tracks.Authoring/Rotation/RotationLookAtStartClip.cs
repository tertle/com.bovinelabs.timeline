// <copyright file="RotationLookAtStartClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using UnityEngine.Timeline;

    public class RotationLookAtStartClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <inheritdoc/>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent<RotationLookAtStart>(clipEntity);
            context.Baker.AddComponent<RotationAnimated>(clipEntity);
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}
