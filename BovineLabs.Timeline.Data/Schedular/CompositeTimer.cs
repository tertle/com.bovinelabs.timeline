// <copyright file="CompositeTimer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;
    using Unity.IntegerTime;

    /// <summary>
    /// Composite timer is a Timer whose time is a transformation from a non-Composite timer
    /// </summary>
    [WriteGroup(typeof(ClockData))]
    public struct CompositeTimer : IComponentData
    {
        /// <summary>The timer this is a transformation of. </summary>
        public Entity SourceTimer;

        /// <summary>Time offset from the source timer</summary>
        public DiscreteTime Offset;

        /// <summary>The scale offset from the source timer</summary>
        public double Scale;

        /// <summary>The range of the source timer. This range will raise the culled flag inside TimerData that reference this</summary>
        public ActiveRange ActiveRange;
    }
}
