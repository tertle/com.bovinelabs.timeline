// <copyright file="Mixers.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;

    public struct FloatMixer : IMixer<float>
    {
        public float Lerp(in float a, in float b, float s) => math.lerp(a, b, s);

        public float Add(in float a, in float b) => a + b;
    }

    public struct Float2Mixer : IMixer<float2>
    {
        public float2 Lerp(in float2 a, in float2 b, float s) => math.lerp(a, b, s);

        public float2 Add(in float2 a, in float2 b) => a + b;
    }

    public struct Float3Mixer : IMixer<float3>
    {
        public float3 Lerp(in float3 a, in float3 b, float s) => math.lerp(a, b, s);

        public float3 Add(in float3 a, in float3 b) => a + b;
    }
}
