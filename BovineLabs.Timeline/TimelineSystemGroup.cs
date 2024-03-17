// <copyright file="TimelineSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using BovineLabs.Core.Groups;
    using Unity.Entities;

    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor, WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(BeforeTransformSystemGroup))]
    public partial class TimelineSystemGroup : ComponentSystemGroup
    {
    }
}
