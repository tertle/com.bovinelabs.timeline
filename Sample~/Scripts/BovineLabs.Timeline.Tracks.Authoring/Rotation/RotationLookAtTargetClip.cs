// <copyright file="RotationLookAtTargetClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Timeline;

    public class RotationLookAtTargetClip : DOTSClip, ITimelineClipAsset
    {
        public ExposedReference<Transform> Target;

        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <inheritdoc/>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            Transform target = null;

            if (context.Director != null)
            {
                target = context.Director.GetReferenceValue(this.Target.exposedName, out _) as Transform;
            }

            context.Baker.AddComponent(clipEntity, new RotationLookAtTarget
            {
                Target = context.Baker.GetEntity(target, TransformUsageFlags.Dynamic),
            });

            context.Baker.AddComponent<RotationAnimated>(clipEntity);
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}
