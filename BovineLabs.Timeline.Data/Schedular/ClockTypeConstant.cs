// <copyright file="ClockTypeConstant.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;
    using Unity.IntegerTime;

    /// <summary>
    /// Component that uses a fixed delta time
    /// </summary>
    [WriteGroup(typeof(ClockData))]
    public struct ClockTypeConstant : IComponentData
    {
        /// <summary>The delta time to advance each frame. </summary>
        public DiscreteTime DeltaTime;

        ///<summary> The timescale to pass to the timer. </summary>
        public double TimeScale;
    }
}
