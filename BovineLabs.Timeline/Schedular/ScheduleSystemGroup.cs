// <copyright file="ScheduleSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Schedular
{
    using BovineLabs.Active;
    using BovineLabs.Reaction;
    using Unity.Entities;

    [UpdateAfter(typeof(ActiveSystemGroup))]
    [UpdateInGroup(typeof(ReactionSystemGroup))]
    public partial class ScheduleSystemGroup : ComponentSystemGroup
    {
    }
}
