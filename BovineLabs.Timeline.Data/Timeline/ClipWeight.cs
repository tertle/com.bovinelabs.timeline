// <copyright file="ClipWeight.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using BovineLabs.Core.Collections;
    using Unity.Entities;

    /// <summary> The current assigned weight of a clip. </summary>
    public struct ClipWeight : IComponentData
    {
        public float Value;
    }

    /// <summary>
    /// Animation Curve for the assigned weight
    /// </summary>
    public struct AnimatedClipWeight : IComponentData
    {
        public BlobCurveSampler Value;
        // public BlobAssetReference<AnimationCurveBlob> Value;
    }
}
