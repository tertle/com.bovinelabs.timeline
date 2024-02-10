// <copyright file="Timer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;
    using Unity.IntegerTime;

    /// <summary> Data used to represent the current state of a timer </summary>
    public struct Timer : IComponentData
    {
        /// <summary>The current time of the timer</summary>
        public DiscreteTime Time;

        /// <summary>The amount the timer advanced. This includes the timescale</summary>
        public DiscreteTime DeltaTime;

        /// <summary>The scale of the timer</summary>
        public double TimeScale;
    }
}
