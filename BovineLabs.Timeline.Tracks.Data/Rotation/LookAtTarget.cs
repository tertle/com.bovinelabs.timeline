// <copyright file="LookAtTarget.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;

    public struct LookAtTarget : IComponentData
    {
        public Entity Target;
    }
}
