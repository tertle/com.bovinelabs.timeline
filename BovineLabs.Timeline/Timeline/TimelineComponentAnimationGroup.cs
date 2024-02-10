// <copyright file="TimelineComponentAnimationGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using Unity.Entities;

    [UpdateInGroup(typeof(TimelineSystemGroup))]
    [UpdateAfter(typeof(ClipWeightSystem))]
    public partial class TimelineComponentAnimationGroup : ComponentSystemGroup
    {
    }
}
