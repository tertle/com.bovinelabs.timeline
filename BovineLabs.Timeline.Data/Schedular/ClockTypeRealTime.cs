// <copyright file="ClockTypeRealTime.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Data.Schedular
{
    using Unity.Entities;

    /// <summary> Component that uses a real time clock update. </summary>
    [WriteGroup(typeof(ClockData))]
    public struct ClockTypeRealTime : IComponentData
    {
    }
}
