// <copyright file="PositionTarget.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct PositionTarget : IComponentData
    {
        public Entity Target;
        public float3 Offset;
        public OffsetType Type;
    }
}