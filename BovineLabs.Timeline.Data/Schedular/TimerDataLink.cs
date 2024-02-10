// <copyright file="TimerDataLink.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;

    [InternalBufferCapacity(0)] // TODO we should figure out a nice value for this
    public struct TimerDataLink : IBufferElementData
    {
        public Entity Value;
    }
}
