// <copyright file="TimeActivePreviousSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Core.Model;
    using BovineLabs.Timeline.Data;
    using Unity.Burst;
    using Unity.Entities;

    /// <summary> System sets the state of <see cref="TimelineActivePrevious" /> when <see cref="TimelineActive" /> changed. </summary>
    [UpdateInGroup(typeof(TimelineSystemGroup), OrderLast = true)]
    public partial struct TimelineActivePreviousSystem : ISystem
    {
        private CopyEnableable<TimelineActivePrevious, TimelineActive> impl;

        public void OnCreate(ref SystemState state)
        {
            this.impl.OnCreate(ref state);
        }

        /// <inheritdoc />
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            this.impl.OnUpdate(ref state);
        }
    }
}
