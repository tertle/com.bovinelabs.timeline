// <copyright file="Float3Mixer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;

    public readonly struct Float3Mixer : IMixer<float3>
    {
        public float3 Lerp(in float3 a, in float3 b, float s) => math.lerp(a, b, s);

        public float3 Add(in float3 a, in float3 b) => a + b;
    }
}
