// <copyright file="ActiveRange.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;
    using Unity.IntegerTime;

    /// <summary>
    /// Component that defines the timer range that an Entity will have an active tag
    /// </summary>
    public struct ActiveRange : IComponentData
    {
        /// <summary> The start of the range (inclusive). </summary>
        public DiscreteTime Start;

        /// <summary> The end of the range (exclusive). </summary>
        public DiscreteTime End;

        /// <summary> A range object representing the full representable range. </summary>
        public static readonly ActiveRange CompleteRange = new()
        {
            Start = DiscreteTime.MinValue,
            End = DiscreteTime.MaxValue,
        };
    }

    /// <summary> Extension Methods for ActiveRange </summary>
    public static class ActiveRangeExtensions
    {
        /// <summary>Returns true the range is valid</summary>
        public static bool IsValid(this ActiveRange range) => range.Start < range.End;

        /// <summary>Return true if the ranges overlap</summary>
        public static bool Overlaps(this ActiveRange range, ActiveRange other) =>
            range.IsValid() && other.IsValid() && range.Start < other.End && other.Start < range.End;

        /// <summary>Returns whether the time is within the range</summary>
        public static bool Contains(this ActiveRange range, DiscreteTime t) => range.Start <= t && range.End > t;

        /// <summary>Returns whether the given range is completely contained within this range</summary>
        public static bool Contains(this ActiveRange range, ActiveRange other) => range.Start <= other.Start && range.End >= other.End;

        /// <summary>The length of the active range.</summary>
        public static DiscreteTime Length(this ActiveRange range) => range.End - range.Start;
    }
}
