// <copyright file="RotationAnimated.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;
    using Unity.Properties;

    public struct RotationAnimated : IAnimatedComponent<quaternion, BlobCurveSampler4<quaternion>>
    {
        [CreateProperty]
        public quaternion DefaultValue { get; set; }

        public BlobCurveSampler4<quaternion> AnimationData { get; set; }
    }
}
