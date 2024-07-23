// <copyright file="PositionAnimated.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;
    using Unity.Properties;

    public struct PositionAnimated : IAnimatedComponent<float3, BlobCurveSampler3>
    {
        [CreateProperty]
        public float3 DefaultValue { get; set; }

        public BlobCurveSampler3 AnimationData { get; set; }
    }
}
