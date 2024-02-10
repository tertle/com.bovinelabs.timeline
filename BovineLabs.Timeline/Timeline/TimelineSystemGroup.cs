// <copyright file="TimelineSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Reaction;
    using BovineLabs.Timeline.Schedular;
    using Unity.Entities;

    [UpdateInGroup(typeof(ReactionSystemGroup))]
    [UpdateAfter(typeof(ScheduleSystemGroup))]
    public partial class TimelineSystemGroup : ComponentSystemGroup
    {
    }
}
