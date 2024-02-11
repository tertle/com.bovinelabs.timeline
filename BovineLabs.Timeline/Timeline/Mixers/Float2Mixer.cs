// <copyright file="Float2Mixer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;

    public readonly struct Float2Mixer : IMixer<float2>
    {
        public float2 Lerp(in float2 a, in float2 b, float s) => math.lerp(a, b, s);

        public float2 Add(in float2 a, in float2 b) => a + b;
    }
}
