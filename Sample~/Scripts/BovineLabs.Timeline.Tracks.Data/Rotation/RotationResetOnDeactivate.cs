// <copyright file="RotationResetOnDeactivate.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Mathematics;

    public struct RotationResetOnDeactivate : IComponentData
    {
        public quaternion Value;
    }
}
