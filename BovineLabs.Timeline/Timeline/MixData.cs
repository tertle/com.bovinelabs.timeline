// <copyright file="MixData.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using Unity.Mathematics;

    public struct MixData<T>
        where T : unmanaged
    {
        public float4 Weights;
        public T Value1;
        public T Value2;
        public T Value3;
        public T Value4;
        public bool Additive;
    }
}
