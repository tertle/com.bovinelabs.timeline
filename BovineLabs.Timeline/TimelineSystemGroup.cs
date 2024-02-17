// <copyright file="TimelineSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline
{
    using Unity.Entities;

    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor, WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class TimelineSystemGroup : ComponentSystemGroup
    {
    }
}
