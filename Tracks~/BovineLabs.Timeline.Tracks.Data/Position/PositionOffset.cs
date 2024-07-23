// <copyright file="PositionOffset.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Mathematics;

    public enum OffsetType : byte
    {
        World,
        Local,
    }

    public struct PositionOffset : IComponentData
    {
        public OffsetType Type;
        public float3 Offset;
    }
}