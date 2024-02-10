// <copyright file="Extrapolation.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data
{
    using Unity.Entities;

    /// <summary> Indicates that a clip holds it's time values outside the clip range. </summary>
    public struct ExtrapolationHold : IComponentData
    {
        public ExtrapolationPosition ExtrapolateOptions;
    }

    /// <summary> Indicates a clip ping pongs it's time values outside it's range. </summary>
    public struct ExtrapolationPingPong : IComponentData
    {
        public ExtrapolationPosition ExtrapolateOptions;
    }

    /// <summary> Indicates a clip loops it's time values outside it's range. </summary>
    public struct ExtrapolationLoop : IComponentData
    {
        public ExtrapolationPosition ExtrapolateOptions;
    }
}
