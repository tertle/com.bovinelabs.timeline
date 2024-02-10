// <copyright file="MovePositionClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Timeline;

    public class MovePositionClip : DOTSClip, ITimelineClipAsset
    {
        public Vector3 Position;

        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <inheritdoc/>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new PositionAnimated { DefaultValue = this.Position });
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}
