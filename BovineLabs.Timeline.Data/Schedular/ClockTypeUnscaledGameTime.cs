// <copyright file="ClockTypeUnscaledGameTime.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;

    /// <summary> Tag Component to make a Timer based on unscaled game time. </summary>
    [WriteGroup(typeof(ClockData))]
    public struct ClockTypeUnscaledGameTime : IComponentData
    {
    }
}
