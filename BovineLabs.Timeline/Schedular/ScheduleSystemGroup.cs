// <copyright file="ScheduleSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using Unity.Entities;

    // [UpdateAfter(typeof(ActiveSystemGroup))]
    // [UpdateInGroup(typeof(ReactionSystemGroup))]
    [UpdateInGroup(typeof(TimelineSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor, WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class ScheduleSystemGroup : ComponentSystemGroup
    {
    }
}
