// <copyright file="TimelineUpdateSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Timeline.Schedular;
    using Unity.Entities;

    [UpdateAfter(typeof(ScheduleSystemGroup))]
    [UpdateInGroup(typeof(TimelineSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor, WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class TimelineUpdateSystemGroup : ComponentSystemGroup
    {
    }
}
