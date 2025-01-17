﻿// <copyright file="DirectorBinding.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using Unity.Collections;
    using Unity.Entities;

    /// <summary> Buffer added to the root director which has a collection of all bindings applied to all tracks. </summary>
    [InternalBufferCapacity(0)]
    public struct DirectorBinding : IBufferElementData
    {
        public FixedString32Bytes TrackIdentifier;
        public Entity TrackEntity;
    }
}
