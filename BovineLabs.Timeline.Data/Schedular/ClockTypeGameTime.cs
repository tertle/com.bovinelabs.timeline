// <copyright file="ClockTypeGameTime.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;

    /// <summary> Tag Component to make a Timer based on game time. </summary>
    [WriteGroup(typeof(ClockData))]
    public struct ClockTypeGameTime : IComponentData
    {
    }
}
