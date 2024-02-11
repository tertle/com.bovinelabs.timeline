// <copyright file="QuaternionMixer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;

    public readonly struct QuaternionMixer : IMixer<quaternion>
    {
        public quaternion Lerp(in quaternion a, in quaternion b, float s) => math.nlerp(a, b, s);

        public quaternion Add(in quaternion a, in quaternion b) => math.mul(a, b);
    }
}
