// <copyright file="TimerData.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;
    using Unity.IntegerTime;

    /// <summary> Timer Data is a copy of the timer data used by entities that react to a timer. </summary>
    public struct TimerData : IComponentData
    {
        /// <summary>The time this frame.</summary>
        public DiscreteTime Time;

        /// <summary>The delta time if the clock this frame.</summary>
        public DiscreteTime DeltaTime;

        /// <summary>The time scale of the timer. </summary>
        public double TimeScale;
    }
}
