// <copyright file="TimeTransform.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using System;
    using Unity.Entities;
    using Unity.IntegerTime;
    using Unity.Mathematics;

    /// <summary>
    /// Which sides of the clip to apply extrapolation
    /// </summary>
    [Flags]
    public enum ExtrapolationPosition
    {
        None = 0,
        Pre = 1,
        Post = 2,
        Both = Pre | Post,
    };

    /// <summary> The transformation from the timer to the local clip space. </summary>
    public struct TimeTransform : IComponentData
    {
        public DiscreteTime start;
        public DiscreteTime end;
        public DiscreteTime clipIn;
        public double scale;

        public static bool operator ==(TimeTransform options1, TimeTransform options2)
        {
            return options1.Equals(options2);
        }

        public static bool operator !=(TimeTransform options1, TimeTransform options2)
        {
            return !options1.Equals(options2);
        }

        public readonly DiscreteTime ToLocalTimeUnbound(DiscreteTime time)
        {
            return ((time - this.start) * this.scale) + this.clipIn;
        }

        public override bool Equals(object obj)
        {
            return obj is TimeTransform transform && this.Equals(transform);
        }

        public bool Equals(TimeTransform other)
        {
            return this.start == other.start && this.end == other.end && this.clipIn == other.clipIn && this.scale == other.scale;
        }

        public override int GetHashCode()
        {
            return math.rol(this.start.GetHashCode(), 1) + math.rol(this.end.GetHashCode(), 7) + math.rol(this.clipIn.GetHashCode(), 12) + math.rol(this.scale.GetHashCode(), 18);
        }
    }
}
