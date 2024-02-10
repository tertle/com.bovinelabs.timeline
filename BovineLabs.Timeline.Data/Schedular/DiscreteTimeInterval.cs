// <copyright file="DiscreteTimeInterval.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IntegerTime;

    /// <summary>
    /// Structure representing a time interval.
    /// </summary>
    public struct DiscreteTimeInterval : IEquatable<DiscreteTimeInterval>, IFormattable
    {
        /// <summary>The largest representable time interval, contains an invalid duration (due to overflow) </summary>
        public static readonly DiscreteTimeInterval MaxRange = new(DiscreteTime.MinValue, DiscreteTime.MaxValue);

        /// <summary>The starting time of the interval</summary>
        public DiscreteTime Start;

        /// <summary>The ending time of the interval</summary>
        public DiscreteTime End;

        /// <summary>Constructor that takes two time values to create an interval. </summary>
        /// <param name="time0"></param>
        /// <param name="time1"></param>
        public DiscreteTimeInterval(DiscreteTime time0, DiscreteTime time1)
        {
            this.Start = time0.Min(time1);
            this.End = time0.Max(time1);
        }

        /// <summary>The duration of the interval</summary>
        /// <remarks>
        /// The duration of the interval will be capped at DiscreteTime.MaxValue, so very large intervals may not satisfy
        ///   end = start + duration
        /// </remarks>
        public DiscreteTime Duration
        {
            get
            {
                // overflow
                if (unchecked(this.End.Value - this.Start.Value) < 0)
                {
                    return DiscreteTime.MaxValue;
                }

                return DiscreteTime.FromTicks(this.End.Value - this.Start.Value);
            }
        }

        /// <summary> The duration of the interval, expressed as a unsigned long </summary>
        /// <remarks>Use this version to if the duration is possibly larger than DiscreteTime.MaxValue</remarks>
        public ulong DurationAsTick => (ulong)this.End.Value - (ulong)this.Start.Value;

        /// <summary>Returns true only if a given time value is inside the interval.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(DiscreteTime t) => t >= this.Start && t <= this.End;

        /// <summary>Returns true only if another interval overlaps.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(DiscreteTimeInterval other) => this.Start <= other.End && other.Start <= this.End;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiscreteTime Clamp(DiscreteTime time) => this.End.Min(this.Start.Max(time));

        /// <summary>Returns true if the time is equal to a given time, false otherwise.</summary>
        public override bool Equals(object o) => this.Equals((DiscreteTimeInterval)o);

        /// <summary>Returns true if the same range is represented.</summary>
        public bool Equals(DiscreteTimeInterval other) => this.Start == other.Start && this.End == other.End;

        /// <summary>Returns a hash code for the time.</summary>
        public override int GetHashCode() => (this.Start.GetHashCode() * 397) ^ this.End.GetHashCode();

        /// <summary>Returns a string representation of the time.</summary>
        public override string ToString()
        {
            return $"{nameof(DiscreteTimeInterval)}({this.Start}, {this.End})";
        }

        /// <summary>Returns a string representation of the time using a specified format and culture-specific format information.</summary>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return
                $"{nameof(DiscreteTimeInterval)}({this.Start.ToString(format, formatProvider)}, {this.End.ToString(format, formatProvider)})";
        }
    }
}
