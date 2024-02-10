// <copyright file="TimerRange.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;

    /// <summary> The behaviour of the timer when it contains a range. </summary>
    public enum RangeBehaviour
    {
        /// <summary> The timer will automatically stop when it reaches the end of the range. </summary>
        AutoStop,

        /// <summary> The timer will automatically pause (i.e. hold) when it reaches the end of the range. </summary>
        AutoPause,

        /// <summary> The timer will loop to the beginning of the range. </summary>
        Loop,
    }

    /// <summary> Component that constrains a timer to a given range. </summary>
    public struct TimerRange : IComponentData
    {
        /// <summary> The behaviour of the timer when the range is applied. </summary>
        public RangeBehaviour Behaviour;

        /// <summary> The time range to use for the timer. </summary>
        public DiscreteTimeInterval Range;

        /// <summary> The number of times the timer has looped. </summary>
        public uint LoopCount;

        /// <summary> Whether the timer should force the last frame to play before disabling. </summary>
        public bool SampleLastFrame;
    }
}
