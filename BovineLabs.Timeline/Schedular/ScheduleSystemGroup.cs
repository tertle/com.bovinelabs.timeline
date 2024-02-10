// <copyright file="ScheduleSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using Unity.Entities;

    // [UpdateAfter(typeof(ActiveSystemGroup))]
    // [UpdateInGroup(typeof(ReactionSystemGroup))]
    [UpdateInGroup(typeof(TimelineSystemGroup))]
    public partial class ScheduleSystemGroup : ComponentSystemGroup
    {
    }
}
